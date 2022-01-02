from posixpath import pardir
from mysql.connector.connection import MySQLConnection
import csv
import os
import datetime
import time
from hashlib import pbkdf2_hmac
import json
from base64 import b64encode
import secrets
os.chdir(os.path.dirname(os.path.realpath(__file__)))
# countries sometimes have different names in the different datasets
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
    "Mainland China": "China",
    "Korea, South": "South Korea",
    "UK": "United Kingdom",
    "Republic of Ireland": "Ireland",
    "St. Martin": "Saint Martin",
    "Iran (Islamic Republic of)": "Iran",
    "Republic of Korea": "South Korea",
    "Hong Kong SAR": "Hong Kong",
    "Taipei and environs": "Taiwan",
    "Viet Nam": "Vietnam",
    "Republic of Moldova": "Moldova",
    "Republic of the Congo": "Congo (Brazzaville)",
    "Gambia, The": "Gambia",
    "Bahamas, The": "Bahamas",
    "The Bahamas": "Bahamas",
    "United States of America": "United States",
    "Bruinei Drusalam": "Bruinei",
    "Saint BarthÃ©lemy": "Saint Barthelemy",
    "RÃ©union": "Reunion",
    "The Gambia": "Gambia",
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
        self.line_num = self.reader.line_num

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
        

def get_number_or_none(num: str):
    try:
        return int(num)
    except:
        return None


def get_number_or_zero(num: str):
    try:
        return int(num)
    except:
        return 0


def multi_execute(connection: MySQLConnection, query: str, params: list):
        cursor = connection.cursor()
        cursor.executemany(query, params)
        connection.commit()
        cursor.close()


def get_country_info(filename: str, country_ids: dict):
    country_info = dict()
    csv_reader = CsvReader(filename)
    country_region = "Country_Region"
    if country_region not in csv_reader.columns_names:
        country_region = "Country/Region"
    for row in csv_reader:
        country = row[country_region]
        country = get_country_name(country)
        if country not in country_ids:
            continue
        if country not in country_info:
            country_info[country] = CData()
        data: CData = country_info[country]
        confirmed_cases = get_number_or_zero(row["Confirmed"])
        deaths = get_number_or_zero(row["Deaths"])
        data.recovered += get_number_or_zero(row["Recovered"])
        data.confirmed += confirmed_cases
        data.deaths += deaths
    csv_reader.close()
    return country_info


def single_execute(connection: MySQLConnection, query: str, params: list = None):
    if params is None:
        params = []
    cursor = connection.cursor()
    cursor.execute(query, params)
    connection.commit()
    cursor.close()


def insert_countries(connection: MySQLConnection):
    """ Insert countries into database from countries-continents.csv
    Args:
        connection (MySQLConnection)
    """
    single_execute(connection, "TRUNCATE TABLE countries")
    query = "INSERT INTO countries (name, continent) VALUES (%s, %s)"
    path = os.path.join("datasets", "Countries-Continents.csv")
    csv_reader = CsvReader(path)
    countries = []
    for row in csv_reader:
        country = get_country_name(row["Country"])
        continent = row["Continent"]
        countries.append((country, continent))
    multi_execute(connection, query, countries)
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


def filename_sort(item: str):
    item = item.replace(".csv", "")
    return datetime.datetime.strptime(item, '%m-%d-%Y').date()


def insert_covid_reports(connection: MySQLConnection):
    single_execute(connection, "TRUNCATE TABLE disease_reports")
    country_ids = get_country_ids(connection)
    folder = os.path.join("datasets", "csse_covid_19_daily_reports")
    query = "INSERT INTO disease_reports (country_id, date, confirmed, deaths, recovered) VALUES (%s, %s, %s, %s, %s)"
    has_recoveries = set()
    files = [filename for filename in os.listdir(folder) if ".csv" in filename]
    files.sort(key=filename_sort)
    for filename in files:
        if not filename.endswith(".csv"):
            continue
        datestr = filename.replace(".csv", "")
        month, day, year = [int(a) for a in datestr.split('-')]
        date = datetime.date(year, month, day).strftime('%Y-%m-%d')
        country_info = get_country_info(os.path.join(folder, filename), country_ids)
        for country, data in country_info.items():
            if data.recovered == 0:
                if country in has_recoveries:
                    data.recovered = None
            elif country not in has_recoveries:
                has_recoveries.add(country)
        params = []
        for country, cdata in country_info.items():
            params.append((country_ids[country], date, cdata.confirmed, cdata.deaths, cdata.recovered))
        multi_execute(connection, query, params)
    


def insert_population_reports(connection: MySQLConnection):
    relevant_years = { 2020, 2021, 2022 }
    single_execute(connection, "TRUNCATE TABLE population_reports")
    path = os.path.join("datasets", "WPP2019_TotalPopulationBySex.csv")
    csv_reader = CsvReader(path)
    country_ids = get_country_ids(connection)
    params = dict()
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
            continue
        population = int(float(row["PopTotal"])*1000)
        density = float(row["PopDensity"])
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
        median = None if row["median_age"] == '' else float(row["median_age"])
        poverty = None if row["extreme_poverty"] == '' else row["extreme_poverty"]
        diabetes = None if row["diabetes_prevalence"] == '' else row["diabetes_prevalence"]
        # if (country_ids[country], year) not in params.keys():
        #     continue
        params[(country_ids[country], year)] += [median, poverty, diabetes]
        visited.add((country, year))
    for params_row in params.values():
        if len(params_row) != 5:
            padding = [None for i in range(5 - len(params_row))]
            params_row += padding
    params = [tuple(list(key) + value) for key, value in params.items()]
    multi_execute(connection, query, params)


def insert_vaccine_reports(connection: MySQLConnection):
    single_execute(connection, "TRUNCATE TABLE vaccine_reports")
    path = os.path.join("datasets", "vaccinations.csv")
    csv_reader = CsvReader(path)
    country_ids = get_country_ids(connection)
    params = []
    batch_max = 500
    fully = dict()
    query = "INSERT INTO vaccine_reports(country_id, date, vaccinated, fully_vaccinated, number_of_boosters) VALUES (%s, %s, %s, %s, %s)"
    for row in csv_reader:
        country = row["location"]
        country = get_country_name(country)
        if country not in country_ids:
            continue
        date = row["date"].replace("/", "-")
        vaccinated = row["people_vaccinated"]
        if vaccinated == '':
            continue
        fully_vaccinated = get_number_or_zero(row["people_fully_vaccinated"])
        if fully_vaccinated != 0:
            fully[country] = fully_vaccinated
        elif country in fully.keys():
            fully_vaccinated = fully[country]
        boosters = get_number_or_zero(row["total_boosters"])
        params.append((country_ids[country], date, vaccinated, fully_vaccinated, boosters))
        if len(params) >= batch_max:
            multi_execute(connection, query, params)
            params.clear()
    multi_execute(connection, query, params)
    csv_reader.close()
    single_execute(connection, "ALTER TABLE vaccine_reports ENABLE KEYS")


# def generate_hashes(connection: MySQLConnection):
#     countries = set()
#     hashes = dict()
#     hashes_rev = dict()
#     for i in range (100000):
#         num = random.randint(4, 8)
#         country = ''.join(random.choices(string.ascii_lowercase, k=num))
#         countries.add(country)
#     count = 1
#     for country in countries:
#         hash = abs(int.from_bytes(sha1(country.encode()).digest()[0:min(len(country), 4)], byteorder='big'))
#         if hash in hashes_rev:
#             print("Collision", count, country, hashes_rev[hash])
#             count += 1
#         else:
#             hashes[country] = hash
#             hashes_rev[hash] = country



def main():
    # sets the current directory to this script's directory
    start = time.time()
    connection = MySQLConnection(user='team11', password='0011', host='localhost', database='db11')
    insert_countries(connection)
    insert_covid_reports(connection)
    insert_population_reports(connection)
    insert_vaccine_reports(connection)
    salt = secrets.token_bytes(12)
    iter = 100000
    password = pbkdf2_hmac('sha1', b'password', salt, iter, dklen=20)
    hash = b64encode(password).decode() + '$' + b64encode(salt).decode() + '$' + str(iter)
    single_execute(connection, "TRUNCATE TABLE managers")
    single_execute(connection, f"INSERT INTO managers (name, password) VALUES ('Yosi', '{hash}')")
    connection.close()
    print(round(time.time() - start), "seconds")
    


main()