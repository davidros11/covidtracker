---CountriesByDate---
-- Shows confirmed cases, deaths, recovered cases, population, number of people vaccinated for each country within the chosen date.
-- Also shows number of new cases over the amount of days specified in the @TH parameter (@TH = 14 means new cases over 14 days, etc)
-- vaccination data is taken from the closest date found before the input date
WITH 
    VD AS (SELECT country_id, MAX(date) as date FROM vaccine_reports  WHERE date <= @DATE GROUP BY country_id),
    VR AS (
            SELECT V.vaccinated, V.fully_vaccinated, V.number_of_boosters, V.country_id, V.date FROM vaccine_reports AS V
            JOIN VD ON VD.country_id = V.country_id AND V.date = VD.date
        )
SELECT
    C.id,
    C.name,
    DR.confirmed,
    DR.confirmed - Prev.confirmed AS new_cases,
    DR.recovered,
    DR.deaths,
    PR.population,
    COALESCE(VR.vaccinated, 0),
    COALESCE(VR.fully_vaccinated, 0),
    COALESCE(VR.number_of_boosters, 0)
FROM disease_reports AS DR
JOIN countries as C
    ON C.id = DR.country_id AND DR.date = @DATE
JOIN population_reports AS PR
    ON PR.year = YEAR(@DATE)
    AND PR.country_id = DR.country_id
JOIN disease_reports AS Prev ON Prev.country_id = DR.country_id AND Prev.date = DATE_SUB(@DATE, INTERVAL @TH DAY)
LEFT JOIN VR
    ON VR.country_id = DR.country_id
---WorldByDate---
-- Shows confirmed cases, deaths, recovered cases, population, number of people vaccinated for the world within the chosen date.
-- Also shows number of new cases over the amount of days specified in the @TH parameter (@TH = 14 means new cases over 14 days, etc)
-- vaccination data is taken from the closest date found before the input date
WITH 
    VD AS (SELECT country_id, MAX(date) as date FROM vaccine_reports  WHERE date <= @DATE GROUP BY country_id),
    VR AS (
            SELECT SUM(V.vaccinated) AS vaccinated, SUM(V.fully_vaccinated) AS fully_vaccinated, SUM(V.number_of_boosters) AS number_of_boosters FROM vaccine_reports AS V
            JOIN VD ON VD.country_id = V.country_id AND V.date = VD.date
        )
SELECT
    SUM(DR.confirmed),
    SUM(DR.confirmed) - (SELECT SUM(confirmed) FROM disease_reports WHERE date = DATE_SUB(@DATE, INTERVAL @TH DAY)) AS new_cases,
    SUM(COALESCE(DR.recovered, 0)),
    SUM(DR.deaths),
    (SELECT SUM(population) FROM population_reports WHERE year = YEAR(@DATE)),
    VR.vaccinated,
    VR.fully_vaccinated,
    VR.number_of_boosters
FROM disease_reports AS DR
CROSS JOIN VR
WHERE DR.date = @DATE
---ContinentsByDate---
-- Shows confirmed cases, deaths, recovered cases, population, number of people vaccinated for each continent within the chosen date.
-- Also shows number of new cases over the amount of days specified in the @TH parameter (@TH = 14 means new cases over 14 days, etc)
-- vaccination data is taken from the closest date found before the input date
WITH 
    VD AS (SELECT country_id, MAX(date) as date FROM vaccine_reports  WHERE date <= @DATE GROUP BY country_id),
    VR AS (
            SELECT V.vaccinated, V.fully_vaccinated, V.number_of_boosters, V.country_id, V.date FROM vaccine_reports AS V
            JOIN VD ON VD.country_id = V.country_id AND V.date = VD.date
        )
SELECT
    C.continent,
    SUM(DR.confirmed),
    SUM(DR.confirmed) - SUM(Prev.confirmed) AS new_cases,
    SUM(COALESCE(DR.recovered, 0)),
    SUM(DR.deaths),
    SUM(PR.population),
    SUM(COALESCE(VR.vaccinated, 0)),
    SUM(COALESCE(VR.fully_vaccinated, 0)),
    SUM(COALESCE(VR.number_of_boosters, 0))
FROM disease_reports AS DR
JOIN countries as C
    ON C.id = DR.country_id AND DR.date = @DATE
JOIN population_reports AS PR
    ON PR.year = YEAR(@DATE)
    AND PR.country_id = DR.country_id
JOIN disease_reports AS Prev ON Prev.country_id = DR.country_id AND Prev.date = DATE_SUB(@DATE, INTERVAL @TH DAY)
LEFT JOIN VR
    ON VR.country_id = DR.country_id
GROUP BY C.continent
---ContinentPopulation---
-- Gives the population of the given continent in the years between @START and @END
SELECT
    PR.year,
    SUM(PR.population)
FROM population_reports AS PR
JOIN countries AS C
    ON C.continent = @CONT
    AND C.id = PR.country_id
GROUP BY PR.year
HAVING PR.year BETWEEN @START AND @END
---ContinentDiseaseData---
-- Gives the disease data of the continent for each date between @START and @END
SELECT
    DR.date,
    SUM(DR.confirmed),
    SUM(DR.deaths),
    SUM(COALESCE(DR.recovered, 0))
FROM disease_reports AS DR
JOIN countries AS C
    ON C.id = DR.country_id
    AND C.continent = @CONT
GROUP BY DR.date
HAVING DR.date BETWEEN @START AND @END
---ContinentVaccineData---
-- Gives the vaccine data of the continent for each date between @START and @END
SELECT
	C.id,
    CEILING(DAY(V.date) / 16) as part,
    MONTH(V.date) as month,
    YEAR(V.date) as year,
	MAX(V.vaccinated) AS vaccinated,
	MAX(V.fully_vaccinated) AS fully_vaccinated,
    MAX(V.number_of_boosters) AS boosters
FROM vaccine_reports AS V
JOIN countries as C
	ON C.continent = @CONT
	AND V.country_id = C.id
WHERE V.date BETWEEN @START AND @END
GROUP BY year, month, part, C.id
ORDER BY year, month, part
---WorldDiseaseData---
-- Gives the disease data of the world for each date between @START and @END
SELECT
    date,
    SUM(confirmed),
    SUM(deaths),
    SUM(COALESCE(recovered, 0))
FROM disease_reports
GROUP BY date
HAVING date BETWEEN @START AND @END
---WorldVaccineData---
-- Gives the vaccine data of the continent for each date between @START and @END
SELECT
	C.id,
    CEILING(DAY(V.date) / 16) as part,
    MONTH(V.date) as month,
    YEAR(V.date) as year,
	MAX(V.vaccinated) AS vaccinated,
	MAX(V.fully_vaccinated) AS fully_vaccinated,
    MAX(V.number_of_boosters) AS boosters
FROM vaccine_reports AS V
JOIN countries as C
	ON V.country_id = C.id
WHERE V.date BETWEEN @START AND @END
GROUP BY year, month, part, C.id
ORDER BY year, month, part
---WorldPopulation---
-- Gives the population of the given continent in the years between @START and @END
SELECT
    year,
    SUM(population)
FROM population_reports
GROUP BY year
HAVING year BETWEEN @START AND @END
---TempDiseaseTable---
-- create temporary table to insert disease data
CREATE TEMPORARY TABLE `temp_disease_reports` (
  `country_name` varchar(128) NOT NULL,
  `confirmed` int NOT NULL,
  `deaths` int NOT NULL,
  `recovered` int DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
---AddDiseaseReports---
-- Add (or update if key exists) disease reports to disease_reports table from temporary table
INSERT INTO disease_reports (country_id, date, confirmed, deaths, recovered)
SELECT * FROM 
(SELECT  C.id AS country_id, @DATE, T.confirmed AS confirmed, T.deaths AS deaths, T.recovered AS recovered
FROM temp_disease_reports AS T JOIN countries AS C ON T.country_name = C.name) AS new
ON DUPLICATE KEY UPDATE confirmed=new.confirmed, deaths = new.deaths, recovered = new.recovered;
---TempVaccineTable---
-- create temporary table to insert disease data
CREATE TEMPORARY TABLE `temp_vaccine_reports` (
  `country_name` varchar(128) NOT NULL,
  `date` DATE NOT NULL,
  `vaccinated` int NOT NULL,
  `fully_vaccinated` int NOT NULL,
  `number_of_boosters` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
---AddVaccineReports---
-- Add (or update if key exists) disease reports to disease_reports table from temporary table
INSERT INTO vaccine_reports (country_id, date, vaccinated, fully_vaccinated, number_of_boosters)
SELECT * FROM 
(SELECT  C.id AS country_id, T.date AS date, T.vaccinated AS vaccinated, T.fully_vaccinated AS fully_vaccinated, T.number_of_boosters AS number_of_boosters
FROM temp_vaccine_reports AS T JOIN countries AS C ON T.country_name = C.name) AS new
ON DUPLICATE KEY UPDATE vaccinated=new.vaccinated, fully_vaccinated = new.fully_vaccinated, number_of_boosters = new.number_of_boosters;