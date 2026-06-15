-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: db101
-- ------------------------------------------------------
-- Server version	8.0.15

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `categories`
--

DROP TABLE IF EXISTS `categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categories` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categories`
--

LOCK TABLES `categories` WRITE;
/*!40000 ALTER TABLE `categories` DISABLE KEYS */;
INSERT INTO `categories` VALUES (1,'Роллы'),(2,'Суши'),(3,'Сеты'),(4,'Пицца'),(5,'Напитки'),(6,'Горячее'),(7,'Десерты');
/*!40000 ALTER TABLE `categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `customers`
--

DROP TABLE IF EXISTS `customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customers` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `phone` varchar(50) NOT NULL,
  `address` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customers`
--

LOCK TABLES `customers` WRITE;
/*!40000 ALTER TABLE `customers` DISABLE KEYS */;
INSERT INTO `customers` VALUES (1,'Иванов Илья Алексеевич','+7(920) 101-01-01','г. Нижний Новгород, ул. Минина, 12'),(2,'Петрова Анна Сергеевна','+7(920) 101-01-02','г. Балахна, ул. Советская, 5'),(3,'Сидоров Кирилл Андреевич','+7(920) 101-01-03','г. Дзержинск, ул. Ленина, 25'),(4,'Сливин Кирилл','+7(904) 043-17-20','г. Балахна, ул. Урицкого, 18'),(5,'Детков Матвей','+7(904) 042-23-51','г. Нижний Новгород, ул. Розы Люксембург, 7'),(6,'Кузнецов Алексей Петрович','+7(920) 555-11-22','г. Нижний Новгород, пр. Гагарина, 45'),(7,'Морозова Екатерина Олеговна','+7(920) 666-33-44','г. Балахна, ул. Чкалова, 3'),(8,'Васильев Дмитрий Игоревич','+7(920) 777-88-99','г. Дзержинск, ул. Гайдара, 14'),(9,'Николаева Светлана Андреевна','+7(920) 888-99-00','г. Нижний Новгород, ул. Бекетова, 8'),(10,'Ковалёв Артём Викторович','+7(920) 999-00-11','г. Балахна, ул. Дзержинского, 22');
/*!40000 ALTER TABLE `customers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_items`
--

DROP TABLE IF EXISTS `order_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_items` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `order_id` bigint(20) DEFAULT NULL,
  `product_id` int(11) DEFAULT NULL,
  `price` decimal(10,2) DEFAULT NULL,
  `quantity` int(11) DEFAULT NULL,
  `sum` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `order_id` (`order_id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `order_items_ibfk_1` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE,
  CONSTRAINT `order_items_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=116 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_items`
--

LOCK TABLES `order_items` WRITE;
/*!40000 ALTER TABLE `order_items` DISABLE KEYS */;
INSERT INTO `order_items` VALUES (1,1,1,390.00,2,780.00),(2,1,16,120.00,2,240.00),(3,1,31,550.00,1,550.00),(4,1,46,120.00,1,120.00),(5,2,6,550.00,2,1100.00),(6,2,3,520.00,2,1040.00),(7,2,25,1500.00,1,1500.00),(8,3,4,390.00,2,780.00),(9,3,8,430.00,2,860.00),(10,3,17,130.00,1,130.00),(11,3,11,450.00,1,450.00),(12,4,1,390.00,3,1170.00),(13,4,9,470.00,2,940.00),(14,4,38,700.00,1,700.00),(15,5,24,1800.00,1,1800.00),(16,5,2,350.00,2,700.00),(17,5,18,150.00,2,300.00),(18,5,47,120.00,2,240.00),(19,5,28,850.00,1,850.00),(20,6,5,340.00,3,1020.00),(21,6,10,410.00,2,820.00),(22,6,50,130.00,1,130.00),(23,7,27,2500.00,1,2500.00),(24,7,13,320.00,2,640.00),(25,7,16,120.00,1,120.00),(26,7,33,650.00,1,650.00),(27,8,12,420.00,2,840.00),(28,8,17,130.00,1,130.00),(29,8,46,120.00,2,240.00),(30,9,26,2100.00,1,2100.00),(31,9,16,120.00,2,240.00),(32,9,15,200.00,2,400.00),(33,9,50,130.00,1,130.00),(34,10,25,1500.00,1,1500.00),(35,10,6,550.00,2,1100.00),(36,10,4,390.00,1,390.00),(37,10,14,330.00,1,330.00),(38,10,32,600.00,1,600.00),(39,10,47,120.00,1,120.00),(40,11,7,520.00,2,1040.00),(41,11,16,120.00,2,240.00),(42,11,12,420.00,1,420.00),(43,11,46,120.00,1,120.00),(44,12,24,1800.00,1,1800.00),(45,12,21,890.00,1,890.00),(46,12,32,600.00,1,600.00),(47,12,47,120.00,2,240.00),(48,12,50,130.00,1,130.00),(49,13,1,390.00,1,390.00),(50,13,9,470.00,1,470.00),(51,13,46,120.00,1,120.00),(52,14,27,2500.00,1,2500.00),(53,14,5,340.00,2,680.00),(54,14,3,520.00,2,1040.00),(55,14,48,120.00,1,120.00),(56,14,50,130.00,1,130.00),(57,15,26,2100.00,1,2100.00),(58,15,18,150.00,1,150.00),(59,15,17,130.00,1,130.00),(60,15,50,130.00,1,130.00),(61,16,21,890.00,1,890.00),(62,16,8,430.00,2,860.00),(63,16,24,1800.00,1,1800.00),(64,16,16,120.00,1,120.00),(65,16,47,120.00,1,120.00),(66,17,11,450.00,2,900.00),(67,17,10,410.00,1,410.00),(68,17,50,130.00,1,130.00),(69,18,24,1800.00,1,1800.00),(70,18,27,2500.00,1,2500.00),(71,18,6,550.00,1,550.00),(72,18,12,420.00,1,420.00),(73,18,48,120.00,1,120.00),(74,19,28,850.00,1,850.00),(75,19,4,390.00,2,780.00),(76,19,2,350.00,1,350.00),(77,19,13,320.00,1,320.00),(78,20,26,2100.00,1,2100.00),(79,20,7,520.00,2,1040.00),(80,20,11,450.00,1,450.00),(81,20,32,600.00,1,600.00),(82,21,5,340.00,2,680.00),(83,21,9,470.00,1,470.00),(84,21,3,520.00,1,520.00),(85,21,46,120.00,1,120.00),(86,22,25,1500.00,1,1500.00),(87,22,4,390.00,2,780.00),(88,22,12,420.00,1,420.00),(89,22,6,550.00,1,550.00),(90,22,32,600.00,1,600.00),(91,23,1,390.00,2,780.00),(92,23,18,150.00,2,300.00),(93,23,50,130.00,1,130.00),(94,24,21,890.00,1,890.00),(95,24,24,1800.00,1,1800.00),(96,24,8,430.00,2,860.00),(97,24,17,130.00,1,130.00),(98,24,46,120.00,1,120.00),(99,24,48,120.00,1,120.00),(100,24,50,130.00,1,130.00),(101,25,28,850.00,1,850.00),(102,25,13,320.00,2,640.00),(103,25,7,520.00,1,520.00),(104,25,16,120.00,1,120.00),(105,25,14,330.00,1,330.00),(106,25,47,120.00,1,120.00),(107,26,1,390.00,10,3900.00),(108,27,22,1200.00,8,9600.00),(109,28,14,330.00,1,330.00),(110,28,6,550.00,1,550.00),(111,28,22,1200.00,1,1200.00),(112,28,50,130.00,1,130.00),(113,29,2,350.00,1,350.00),(115,30,1,390.00,1,390.00);
/*!40000 ALTER TABLE `order_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_statuses`
--

DROP TABLE IF EXISTS `order_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_statuses` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_statuses`
--

LOCK TABLES `order_statuses` WRITE;
/*!40000 ALTER TABLE `order_statuses` DISABLE KEYS */;
INSERT INTO `order_statuses` VALUES (1,'Новый'),(3,'Завершен');
/*!40000 ALTER TABLE `order_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `customer_id` int(11) DEFAULT NULL,
  `order_date` datetime DEFAULT NULL,
  `status_id` int(11) DEFAULT NULL,
  `total` decimal(10,2) DEFAULT NULL,
  `discount` decimal(10,2) DEFAULT NULL,
  `final_total` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `customer_id` (`customer_id`),
  KEY `status_id` (`status_id`),
  CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`),
  CONSTRAINT `orders_ibfk_2` FOREIGN KEY (`status_id`) REFERENCES `order_statuses` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orders`
--

LOCK TABLES `orders` WRITE;
/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
INSERT INTO `orders` VALUES (1,1,'2026-05-01 12:30:00',3,1640.00,0.00,1640.00),(2,2,'2026-05-01 14:15:00',3,3500.00,525.00,2975.00),(3,3,'2026-05-02 13:00:00',1,2090.00,0.00,2090.00),(4,4,'2026-05-02 18:45:00',3,2750.00,0.00,2750.00),(5,5,'2026-05-03 11:20:00',1,4200.00,630.00,3570.00),(6,6,'2026-05-03 15:30:00',3,1900.00,0.00,1900.00),(7,7,'2026-05-04 19:00:00',1,3640.00,546.00,3094.00),(8,8,'2026-05-04 20:15:00',3,1210.00,0.00,1210.00),(9,9,'2026-05-05 12:00:00',1,2900.00,0.00,2900.00),(10,10,'2026-05-05 16:30:00',3,4100.00,615.00,3485.00),(11,1,'2026-05-06 11:00:00',1,1850.00,0.00,1850.00),(12,2,'2026-05-06 14:45:00',3,3600.00,540.00,3060.00),(13,3,'2026-05-07 13:20:00',1,980.00,0.00,980.00),(14,4,'2026-05-07 18:00:00',3,4500.00,675.00,3825.00),(15,5,'2026-05-08 12:30:00',1,2450.00,0.00,2450.00),(16,6,'2026-05-08 15:45:00',3,3780.00,567.00,3213.00),(17,7,'2026-05-09 14:00:00',1,1500.00,0.00,1500.00),(18,8,'2026-05-09 19:30:00',3,5400.00,810.00,4590.00),(19,9,'2026-05-10 13:00:00',1,2340.00,0.00,2340.00),(20,10,'2026-05-10 17:00:00',3,4200.00,630.00,3570.00),(21,1,'2026-05-11 11:30:00',1,1700.00,0.00,1700.00),(22,2,'2026-05-11 15:15:00',3,3650.00,548.00,3102.00),(23,3,'2026-05-12 12:00:00',1,1100.00,0.00,1100.00),(24,4,'2026-05-12 16:30:00',3,4000.00,600.00,3400.00),(25,5,'2026-05-13 13:00:00',1,2620.00,0.00,2620.00),(26,6,'2026-05-13 17:30:00',3,3900.00,585.00,3315.00),(27,7,'2026-05-14 19:00:00',1,9600.00,1440.00,8160.00),(28,8,'2026-05-14 20:00:00',3,2110.00,0.00,2110.00),(29,9,'2026-05-15 20:00:00',1,350.00,0.00,350.00),(30,10,'2026-05-15 21:00:00',1,390.00,0.00,390.00);
/*!40000 ALTER TABLE `orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `article` varchar(50) DEFAULT NULL,
  `name` varchar(100) DEFAULT NULL,
  `description` text,
  `price` decimal(10,2) DEFAULT NULL,
  `category_id` int(11) DEFAULT NULL,
  `image_path` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `category_id` (`category_id`),
  CONSTRAINT `products_ibfk_1` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES (1,'111111','Филадельфия классик','Лосось, сливочный сыр, огурец',390.00,1,'filclas.jpeg'),(2,'111112','Филадельфия лайт','Лосось, сливочный сыр',350.00,1,'fillight.jpeg'),(3,'111113','Филадельфия с угрем','Лосось, угорь, сыр',520.00,1,'filugr.jpeg'),(4,'111114','Калифорния с лососем','Лосось, авокадо, икра тобико',390.00,1,'califlos.jpeg'),(5,'111115','Калифорния классик','Краб, авокадо, икра тобико',340.00,1,'califcrab.jpg'),(6,'111116','Дракон спайси','Угорь, авокадо, соус спайси',550.00,1,'hotdracon.jpeg'),(7,'111117','Драгон классик','Угорь, авокадо, унаги соус',520.00,1,'draconclas.jpeg'),(8,'111118','Бонито ролл','Тунец, стружка тунца',430.00,1,'bonito.png'),(9,'111119','Темпура ролл','Креветка в кляре, авокадо',470.00,1,'tempura.jpeg'),(10,'111120','Спайси туна','Тунец, острый соус, огурец',410.00,1,'hotrolltunec.jpeg'),(11,'111121','Окинава','Креветка, сливочный сыр, авокадо',450.00,1,'okinavakrevetka.jpg'),(12,'111122','Ролл с угрем','Угорь, огурец, соус унаги',420.00,1,'ygrogyrec.png'),(13,'111123','Ролл с лососем','Лосось, рис, нори',320.00,1,'nigirilososo.jpg'),(14,'111124','Ролл с тунцом','Тунец, рис, нори',330.00,1,'nigiritynec.jpg'),(15,'111125','Ролл огурец','Огурец, кунжут',200.00,1,'ogyreckynjyt.jpeg'),(16,'222221','Суши лосось','Свежий лосось на рисе',120.00,2,'nigirilococ222.jpg'),(17,'222222','Суши тунец','Свежий тунец на рисе',130.00,2,'nigiritynec222.jpg'),(18,'222223','Суши угорь','Копченый угорь на рисе',150.00,2,'nigirigrebeshok.jpg'),(19,'222224','Суши креветка','Креветка на рисе',110.00,2,'nigirikrevetka.jpg'),(20,'222225','Суши осьминог','Осьминог на рисе',160.00,2,'nigiriosminog.jpg'),(21,'222226','Суши морской гребешок','Гребешок на рисе',170.00,2,'Hotgrebesok.jpeg'),(22,'222227','Суши икра','Икра летучей рыбы',100.00,2,'UNAGO.jpg'),(23,'222228','Суши унаги','Угорь в соусе унаги',180.00,2,'tobiko.jpeg'),(24,'222229','Суши гребешок спайси','Гребешок с острым соусом',190.00,2,'grebenhohothtohotoht.jpg'),(25,'222230','Суши ассорти 6 шт','Лосось, тунец, угорь (по 2 шт)',700.00,2,'miniset.jpeg'),(26,'333331','Сет Мини','16 шт роллов',890.00,3,'sushiassorti.jpg'),(27,'333332','Сет Классик','24 шт роллов',1200.00,3,'24stuk.jpg'),(28,'333333','Сет Гурман','32 шт роллов',1650.00,3,'gurme.jpeg'),(29,'333334','Сет Филадельфия','Филадельфия микс (30 шт)',1800.00,3,'filasetttt.jpg'),(30,'333335','Сет Темпура','Жареные роллы (24 шт)',1500.00,3,'tempuraset.jpeg'),(31,'333336','Сет Макси','40 шт роллов',2100.00,3,'maxiset.jpeg'),(32,'333337','Сет Премиум','48 шт роллов',2500.00,3,'premiumset.png'),(33,'333338','Сет Романтик','16 шт роллов на двоих',850.00,3,'parniset.jpg'),(34,'333339','Сет Студент','20 шт роллов',999.00,3,'studentset.jpg'),(35,'333340','Сет Семейный','72 шт роллов',3900.00,3,'familyset.jpg'),(36,'444441','Пицца Маргарита','Томатный соус, моцарелла, базилик',550.00,4,'margarita.jpeg'),(37,'444442','Пицца Пепперони','Пепперони, моцарелла, томатный соус',600.00,4,'peperoni.jpg'),(38,'444443','Пицца 4 сыра','Моцарелла, пармезан, дорблю, чеддер',650.00,4,'4sira.jpg'),(39,'444444','Пицца Гавайская','Курица, ананас, моцарелла',620.00,4,'gavai.jpeg'),(40,'444445','Пицца Барбекю','Говядина, бекон, соус',680.00,4,'bbq.jpeg'),(41,'444446','Пицца Вегетарианская','Помидоры, перец, грибы, оливки',550.00,4,'vegan.jpeg'),(42,'444447','Пицца Мясная','Бекон, ветчина, пепперони, говядина',700.00,4,'meathappy.jpg'),(43,'444448','Пицца Острая','Перец халапеньо, мясо, моцарелла',720.00,4,'hotpIZZA.jpeg'),(44,'444449','Пицца Куриная','Курица, грибы, моцарелла',650.00,4,'chickengrib.jpeg'),(45,'444450','Пицца Сырная','4 вида сыра, чеснок',600.00,4,'chesse.jpg'),(46,'555551','Кока-Кола','0.5л, газированная',120.00,5,'cola.jpg'),(47,'555552','Фанта','0.5л, апельсиновая',120.00,5,'fanta.jpeg'),(48,'555553','Спрайт','0.5л, лимон-лайм',120.00,5,'sprite.jpg'),(49,'555554','Сок апельсиновый','0.5л, 100% натуральный',150.00,5,'orangejuice.jpg'),(50,'555555','Зелёный чай','0.5л, в бутылке',130.00,5,'tea.jpg');
/*!40000 ALTER TABLE `products` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'Администратор'),(2,'Директор'),(3,'Менеджер');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `full_name` varchar(100) NOT NULL,
  `login` varchar(50) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `role_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `login` (`login`),
  KEY `role_id` (`role_id`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'Кирилл Сливин','1','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b',1),(2,'Директор','2','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b',2),(3,'Менеджер','3','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b',3);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-06-13  1:58:34
