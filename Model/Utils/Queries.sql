---ByDate---
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
---WorldDateData---
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
---ContinentByDate---
WITH 
    VD AS (SELECT country_id, MAX(date) as date FROM vaccine_reports  WHERE date <= @DATE GROUP BY country_id),
    VR AS (
            SELECT V.vaccinated, V.fully_vaccinated, V.number_of_boosters, V.country_id, V.date FROM vaccine_reports AS V
            JOIN VD ON VD.country_id = V.country_id AND V.date = VD.date
        )
SELECT
    C.id,
    C.name,
    SUM(DR.confirmed),
    SUM(DR.confirmed - Prev.confirmed AS new_cases),
    SUM(COALESCE(DR.recovered, 0)),
    SUM(DR.deaths),
    PR.population,
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
---GetPopulationData---
SELECT
    C.id,
    C.name,
    PR.population,
    PR.density,
    PR.median_age,
    PR.poverty_rate,
    PR.diabetes_rate
FROM countries AS PR
JOIN population_reports AS PR
    ON C.id = PR.country_id
    AND PR.year = @YEAR
---ContinentDiseaseData---
SELECT
    date,
    SUM(confirmed),
    SUM(deaths),
    SUM(COALESCE(DR.recovered, 0))
FROM disease_reports AS DR
JOIN countries AS C
    ON C.id = DR.country_id
    AND C.continent = @CONT
GROUP BY DR.date
