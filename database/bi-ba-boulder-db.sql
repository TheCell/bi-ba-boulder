-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Oct 07, 2024 at 06:38 PM
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
  `id` int(11) NOT NULL,
  `boulderBlocId` int(11) NOT NULL,
  `resolutionLevel` tinyint(4) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `boulderbloc`
--

CREATE TABLE `boulderbloc` (
  `id` int(11) NOT NULL,
  `name` varchar(1000) NOT NULL,
  `description` varchar(1000) DEFAULT NULL,
  `boulderSectorId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `boulderline`
--

CREATE TABLE `boulderline` (
  `id` int(11) NOT NULL,
  `color` varchar(9) DEFAULT NULL,
  `name` varchar(1000) NOT NULL,
  `description` varchar(1000) DEFAULT NULL,
  `identifier` varchar(1000) NOT NULL,
  `grade` varchar(255) NOT NULL,
  `boulderBlocId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `bouldersector`
--

CREATE TABLE `bouldersector` (
  `id` int(11) NOT NULL,
  `name` varchar(1000) NOT NULL,
  `description` varchar(1000) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `point`
--

CREATE TABLE `point` (
  `id` int(11) NOT NULL,
  `x` double NOT NULL,
  `y` double NOT NULL,
  `z` double NOT NULL,
  `boulderLineId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `blocfile`
--
ALTER TABLE `blocfile`
  ADD PRIMARY KEY (`id`),
  ADD KEY `blocfile.BoulderBlocId` (`boulderBlocId`);

--
-- Indexes for table `boulderbloc`
--
ALTER TABLE `boulderbloc`
  ADD PRIMARY KEY (`id`),
  ADD KEY `boulderbloc.BoulderSectorId` (`boulderSectorId`);

--
-- Indexes for table `boulderline`
--
ALTER TABLE `boulderline`
  ADD PRIMARY KEY (`id`),
  ADD KEY `boulderline.BoulderBlocId` (`boulderBlocId`);

--
-- Indexes for table `bouldersector`
--
ALTER TABLE `bouldersector`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `point`
--
ALTER TABLE `point`
  ADD PRIMARY KEY (`id`),
  ADD KEY `point.BoulderLineId` (`boulderLineId`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `blocfile`
--
ALTER TABLE `blocfile`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `boulderbloc`
--
ALTER TABLE `boulderbloc`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `boulderline`
--
ALTER TABLE `boulderline`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `bouldersector`
--
ALTER TABLE `bouldersector`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `point`
--
ALTER TABLE `point`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `blocfile`
--
ALTER TABLE `blocfile`
  ADD CONSTRAINT `blocfile.BoulderBlocId` FOREIGN KEY (`BoulderBlocId`) REFERENCES `boulderbloc` (`Id`);

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
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
