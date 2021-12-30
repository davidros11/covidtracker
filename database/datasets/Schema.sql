CREATE TABLE `countries` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(60) NOT NULL,
  `continent` varchar(45) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `cont` (`continent`)
) ENGINE=InnoDB AUTO_INCREMENT=189 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `disease_reports` (
  `country_id` int NOT NULL,
  `date` date NOT NULL,
  `confirmed` int NOT NULL,
  `deaths` int NOT NULL,
  `recovered` int DEFAULT NULL,
  PRIMARY KEY (`country_id`,`date`),
  KEY `just_date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `managers` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  `password` varchar(64) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `population_reports` (
  `country_id` int NOT NULL,
  `year` int NOT NULL,
  `population` int NOT NULL,
  `density` float DEFAULT NULL,
  `median_age` float DEFAULT NULL,
  `poverty_rate` float DEFAULT NULL,
  `diabetes_rate` float DEFAULT NULL,
  PRIMARY KEY (`country_id`,`year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `vaccine_reports` (
  `country_id` int NOT NULL,
  `date` date NOT NULL,
  `vaccinated` int NOT NULL,
  `fully_vaccinated` int NOT NULL,
  `number_of_boosters` int NOT NULL,
  PRIMARY KEY (`country_id`,`date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
