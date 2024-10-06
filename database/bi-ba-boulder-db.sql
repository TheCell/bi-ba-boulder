-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Oct 06, 2024 at 01:27 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `bi-ba-boulder-db`
--

-- --------------------------------------------------------

--
-- Table structure for table `blocfile`
--

CREATE TABLE `blocfile` (
  `Id` VARBINARY(16) NOT NULL,
  `BoulderBlocId` VARBINARY(16) NOT NULL,
  `ResolutionLevel` tinyint(4) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `boulderbloc`
--

CREATE TABLE `boulderbloc` (
  `Id` VARBINARY(16) NOT NULL,
  `Name` varchar(1000) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `BoulderSectorId` VARBINARY(16) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `boulderline`
--

CREATE TABLE `boulderline` (
  `Id` VARBINARY(16) NOT NULL,
  `Color` varchar(9) DEFAULT NULL,
  `Name` varchar(1000) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `Identifier` varchar(1000) NOT NULL,
  `Grade` varchar(255) NOT NULL,
  `BoulderBlocId` VARBINARY(16) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `bouldersector`
--

CREATE TABLE `bouldersector` (
  `Id` VARBINARY(16) NOT NULL,
  `Name` varchar(1000) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `point`
--

CREATE TABLE `point` (
  `Id` VARBINARY(16) NOT NULL,
  `X` double NOT NULL,
  `Y` double NOT NULL,
  `Z` double NOT NULL,
  `BoulderLineId` VARBINARY(16) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `blocfile`
--
ALTER TABLE `blocfile`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `boulderbloc`
--
ALTER TABLE `boulderbloc`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `BoulderSectorId` (`BoulderSectorId`);

--
-- Indexes for table `boulderline`
--
ALTER TABLE `boulderline`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `BoulderBlocId` (`BoulderBlocId`);

--
-- Indexes for table `bouldersector`
--
ALTER TABLE `bouldersector`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `point`
--
ALTER TABLE `point`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `BoulderLineId` (`BoulderLineId`);

--
-- Constraints for dumped tables
--

--
-- Constraints for table `boulderbloc`
--
ALTER TABLE `boulderbloc`
  ADD CONSTRAINT `boulderbloc.BoulderSectorId` FOREIGN KEY (`BoulderSectorId`) REFERENCES `bouldersector` (`Id`);

--
-- Constraints for table `boulderline`
--
ALTER TABLE `boulderline`
  ADD CONSTRAINT `boulderline.BoulderBlocId` FOREIGN KEY (`BoulderBlocId`) REFERENCES `boulderbloc` (`Id`);

--
-- Constraints for table `point`
--
ALTER TABLE `point`
  ADD CONSTRAINT `point.BoulderLineId` FOREIGN KEY (`BoulderLineId`) REFERENCES `boulderline` (`Id`);

--
-- Constraints for table `blocfile`
--
ALTER TABLE `blocfile`
  ADD CONSTRAINT `blocfile.BoulderBlocId` FOREIGN KEY (`BoulderBlocId`) REFERENCES `boulderbloc` (`Id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
