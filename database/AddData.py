from mysql.connector.connection import MySQLConnection
import csv
import os
import datetime
import time
alternate_country_names = {
    "Russian Federation": "Russia",
    "Congo": "Congo (Brazzaville)",
    "DR Congo": "Congo (Kinshasa)",
    "Democratic Republic of the Congo": "Congo (Kinshasa)",
    "Congo, Democratic Republic of": "Congo (Kinshasa)",
    "Burkina": "Burkina Faso",
    "Burma (Myanmar)": "Burma",
    "Myanmar": "Burma",
    "Cabo Verde": "Cape Verde",
    "Cote d'Ivoire": "Ivory Coast",
    "CZ": "Czech Republic",
    "Czechia": "Czech Republic",
    "Eswatini": "Swaziland",
    "Holy See": "Vatican City",
    "Macedonia": "North Macedonia",
    "Timor-Leste": "East Timor",
    "West Bank and Gaza": "Israel",
    "Mainland China": "China",
    "Korea, South": "South Korea",
    "UK": "United Kingdom", 
    "North Ireland": "Ireland",
    "Northern Ireland": "Ireland",
    "Republic of Ireland": "Ireland",
    "St. Martin": "Saint Martin",
    "Iran (Islamic Republic of)": "Iran",
    "Republic of Korea": "South Korea",
    "Hong Kong SAR": "Hong Kong",
    "Taipei and environs": "Taiwan",
    "Viet Nam": "Vietnam",
    "Republic of Moldova": "Moldova",
    "Republic of the Congo": "Congo (Brazzaville)",
    "Gambia, The": "The Gambia",
    "Bahamas, The": "The Bahamas",
    "United States of America": "United States",
    "Bruinei Drusalam": "Bruinei",
    "Saint BarthÃ©lemy": "Saint Barthelemy",
    "RÃ©union": "Reunion",
    "United Republic of Tanzania": "Tanzania",
    "Syrain Arab Republic": "Syria",
    "Venezuela (Bolivarian Republic of)": "Venezuela",
    "Saint Martin (French part)": "Saint Martin",
    "Micronesia (Fed. States of)":"Micronesia",
    "Dem. People's Republic of Korea": "North Korea",
    "CÃ´te d'Ivoire": "Ivory Coast",
    "CuraÃ§ao": "Curacao",
    "China, Hong Kong SAR": "Hong Kong",
    "China, Taiwan Province of China": "Taiwan",
    "China, Macao SAR": "Macau",
    "Lao People's Democratic Republic": "Laos",
    "US": "United States",
    "Democratic Republic of Congo": "Congo (Kinshasa)",
    "Faeroe Islands": "Faroe Islands",
    "Bolivia (Plurinational State of)": "Bolivia",
    "Brunei Darussalam": "Brunei",
    "Syrian Arab Republic": "Syria",
    "Vatican": "Vatican City"
}

class CData:
    confirmed = 0
    deaths = 0
    recovered = 0


class CsvReader:
    def __init__(self, path: str):
        self.file = open(path)
        self.reader = csv.reader(self.file)
        self.columns_names = next(self.reader)

    def close(self):
        self.file.close()
    
    def __iter__(self):
        return self
    
    def __next__(self):
        row = next(self.reader)
        return { self.columns_names[i]: cell for i, cell in enumerate(row) }


def get_country_name(country):
    country = country.replace('*', '').strip()
    if country in alternate_country_names:
        return alternate_country_names[country]
    return country
        

def get_number(num: str):
    try:
        return int(num)
    except:
        return 0

def get_country_info(filename: str, country_ids: dict):
    country_info = dict()
    csv_reader = CsvReader(filename)
    country_region = "Country_Region"
    discarded = set()
    if country_region not in csv_reader.columns_names:
        country_region = "Country/Region"
    for row in csv_reader:
        country = row[country_region]
        country = get_country_name(country)
        if country not in country_ids:
            discarded.add(country)
            continue
        if country not in country_info:
            country_info[country] = CData()
        data: CData = country_info[country]
        confirmed_cases = row["Confirmed"]
        deaths = row["Deaths"]
        recoveries = row["Recovered"]
        data.confirmed += get_number(confirmed_cases)
        data.deaths += get_number(deaths)
        data.recovered += get_number(recoveries)
    csv_reader.close()
    return country_info, discarded


def execute_single_query(connection: MySQLConnection, query: str):
    cursor = connection.cursor()
    cursor.execute(query)
    connection.commit()
    cursor.close()


def insert_countries(connection: MySQLConnection):
    execute_single_query(connection, "TRUNCATE TABLE countries")
    query = "INSERT INTO countries (name, continent) VALUES (%s, %s)"
    path = os.path.join("datasets", "Countries-Continents.csv")
    csv_reader = CsvReader(path)
    cursor = connection.cursor()
    countries = []
    for row in csv_reader:
        country = get_country_name(row["Country"])
        continent = row["Continent"]
        countries.append((country, continent))
    cursor.executemany(query, countries)
    connection.commit()
    cursor.close()
    csv_reader.close()
    

def get_country_ids(connection: MySQLConnection):
    country_ids = dict()
    cursor = connection.cursor()
    query = "SELECT id, name FROM countries"
    cursor.execute(query)
    for row in cursor:
        country_ids[row[1]] = row[0]
    length = 0
    connection.commit()
    cursor.close()
    return country_ids


def insert_covid_reports(connection: MySQLConnection):
    execute_single_query(connection, "TRUNCATE TABLE disease_reports")
    country_ids = get_country_ids(connection)
    folder = os.path.join("datasets", "csse_covid_19_daily_reports")
    query = "INSERT INTO disease_reports (country_id, date, confirmed, deaths, recovered) VALUES (%s, %s, %s, %s, %s)"
    discarded = set()
    for filename in os.listdir(folder):
        cursor = connection.cursor()
        if not filename.endswith(".csv"):
            continue
        datestr = filename.replace(".csv", "")
        month, day, year = [int(a) for a in datestr.split('-')]
        date = datetime.date(year, month, day).strftime('%Y-%m-%d')
        country_info, new_discarded = get_country_info(os.path.join(folder, filename), country_ids)
        discarded = discarded.union(new_discarded)
        varias = [(country_ids[country], date, cdata.confirmed, cdata.deaths, cdata.recovered) for country, cdata in country_info.items()]
        cursor.executemany(query, varias)
        connection.commit()
        cursor.close()
    print(*discarded, sep='\n')
    


def insert_population_reports(connection: MySQLConnection):
    relevant_years = { 2019, 2020, 2021 }
    execute_single_query(connection, "TRUNCATE TABLE population_reports")
    path = os.path.join("datasets", "WPP2019_TotalPopulationBySex.csv")
    csv_reader = CsvReader(path)
    country_ids = get_country_ids(connection)
    params = dict()
    discarded = set()
    for row in csv_reader:
        year = int(row["Time"])
        if year not in relevant_years:
            continue
        variant = row["Variant"]
        if variant != "No change":
            continue
        country = row["Location"]
        country = get_country_name(country)
        if country not in country_ids:
            if country not in discarded:
                print(country)
                discarded.add(country)
            continue
        population = int(float(row["PopTotal"])*1000)
        density = float(row["PopDensity"])
        if country == "Bolivia":
            print(year, "--------------------------------------------------------------------------------------------------------")
        params[(country_ids[country], year)] = [population, density]
    csv_reader.close()
    path = os.path.join("datasets", "owid-covid-data.csv")
    csv_reader = CsvReader(path)
    visited = set()
    query = "INSERT INTO population_reports(country_id, year, population, density, median_age, poverty_rate, diabetes_Rate)\
            VALUES (%s, %s, %s, %s, %s, %s, %s)"
    for row in csv_reader:
        country = row["location"]
        country = get_country_name(country)
        year = int(row["date"].split('-')[0])
        if country not in country_ids:
            continue
        if((country, year) in visited):
            continue
        median = 0 if row["median_age"] == '' else float(row["median_age"])
        poverty = 0 if row["extreme_poverty"] == '' else row["extreme_poverty"]
        diabetes = 0 if row["diabetes_prevalence"] == '' else row["diabetes_prevalence"]
        params[(country_ids[country], year)] += [median, poverty, diabetes]
        visited.add((country, year))
    cursor = connection.cursor()
    rev_ids = { value: key for key, value in country_ids.items()}
    params = [tuple(list(key) + value) for key, value in params.items()]
    print("_______________________________________________________________________________________")
    for par in params:
        if len(par) != 7:
            print(rev_ids[par[0]])
    cursor.executemany(query, params)
    connection.commit()
    cursor.close()


def insert_vaccine_reports(connection: MySQLConnection):
    execute_single_query(connection, "TRUNCATE TABLE vaccine_reports")
    path = os.path.join("datasets", "vaccinations.csv")
    csv_reader = CsvReader(path)
    country_ids = get_country_ids(connection)
    params = []
    batch_max = 500
    query = "INSERT INTO vaccine_reports(country_id, date, vaccinated, fully_vaccinated, number_of_boosters) VALUES (%s, %s, %s, %s, %s)"
    discard = set()
    count = 0
    for row in csv_reader:
        country = row["location"]
        country = get_country_name(country)
        if country not in country_ids:
            if country not in discard:
                print(country)
                discard.add(country)
            continue
        date = row["date"].replace("/", "-")
        vaccinated = get_number(row["people_vaccinated"])
        fully_vaccinated = get_number(row["people_fully_vaccinated"])
        boosters = get_number(row["total_boosters"])
        params.append((country_ids[country], date, vaccinated, fully_vaccinated, boosters))
        count += 1
        if count == batch_max:
            cursor = connection.cursor()
            cursor.executemany(query, params)
            connection.commit()
            cursor.close()
            count = 0
            params.clear()
    csv_reader.close()

def main():
    # sets the current directory to this script's directory
    os.chdir(os.path.dirname(os.path.realpath(__file__)))
    start = time.time()
    connection = MySQLConnection(user='root', password='passwordius99', host='127.0.0.1', database='diseasetracker')
    insert_countries(connection)
    insert_covid_reports(connection)
    print("------------------------------------------------------")
    insert_population_reports(connection)
    print("------------------------------------------------------")
    insert_vaccine_reports(connection)
    connection.close()
    print(round(time.time() - start), "seconds")


main()