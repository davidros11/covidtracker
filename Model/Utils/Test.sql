DECLARE @DATE Date = '2020-10-19';
WITH VD AS (SELECT country_id, MAX(date) as date FROM vaccine_reports GROUP BY country_id HAVING date <= @DATE),
     VR AS (
            SELECT V.vaccinated, V.fully_vaccinated, V.country_id, date FROM vaccine_reports AS V
            JOIN VD ON VD.country_id = V.country_id AND V.date = VD.date
        )
SELECT
    C.id,
    C.name,
    DR.confirmed,
    DR.recovered,
    DR.deaths,
    DR.confirmed - DR.recovered - DR.deaths,
    PR.population,
    VR.vaccinated,
    VR.fully_vaccinated
FROM disease_reports AS DR
JOIN countries as C
    ON C.id = DR.country_id AND DR.date = @DATE
JOIN population_reports AS PR
    ON PR.year = YEAR(@DATE)
LEFT JOIN VR
    ON VR.country_id = DR.country_id