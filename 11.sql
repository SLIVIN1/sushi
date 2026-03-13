-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
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
  `is_deleted` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categories`
--

LOCK TABLES `categories` WRITE;
/*!40000 ALTER TABLE `categories` DISABLE KEYS */;
INSERT INTO `categories` VALUES (1,'Роллы',0),(2,'Суши',0),(3,'Сеты',0),(4,'Салаты',0),(5,'Напитки',0),(6,'Горячее',0),(7,'Десерты',0),(8,'Пицца',0);
/*!40000 ALTER TABLE `categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_items`
--

DROP TABLE IF EXISTS `order_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_items` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `order_id` bigint(20) NOT NULL,
  `product_name` varchar(255) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `quantity` int(11) NOT NULL,
  `sum` decimal(10,2) NOT NULL,
  `product_id` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `order_id` (`order_id`),
  CONSTRAINT `order_items_ibfk_1` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=158 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_items`
--

LOCK TABLES `order_items` WRITE;
/*!40000 ALTER TABLE `order_items` DISABLE KEYS */;
INSERT INTO `order_items` VALUES (88,18,'Сет Самурай',1200.00,1,1200.00,'135'),(89,18,'Саке',350.00,1,350.00,'149'),(90,18,'Суши лосось',120.00,1,120.00,'127'),(91,18,'Вода без газа',90.00,1,90.00,'154'),(92,19,'Филадельфия классик',390.00,2,780.00,'117'),(93,19,'Зеленый чай',150.00,1,150.00,'150'),(94,19,'Суши лосось',120.00,1,120.00,'127'),(95,20,'Сет Самурай',1200.00,1,1200.00,'135'),(96,20,'Сет Токио',850.00,1,850.00,'136'),(97,20,'Моти',200.00,1,200.00,'155'),(98,21,'Сет Премиум',2100.00,1,2100.00,'139'),(99,21,'Сет Осака',1550.00,1,1550.00,'137'),(100,21,'Сет Классика',1290.00,1,1290.00,'141'),(101,22,'Сет Токио',850.00,1,850.00,'136'),(102,23,'Сет Самурай',1200.00,1,1200.00,'135'),(103,23,'Вода без газа',90.00,1,90.00,'154'),(104,24,'Сет Самурай',1200.00,1,1200.00,'135'),(105,24,'Сет Токио',850.00,1,850.00,'136'),(106,25,'Пицца Маргарита',550.00,1,550.00,'143'),(107,25,'Вода без газа',90.00,2,180.00,'154'),(108,25,'Вода без газа',90.00,1,90.00,'154'),(109,26,'Сет Самурай',1200.00,2,2400.00,'135'),(110,26,'Сет Токио',850.00,1,850.00,'136'),(111,27,'Саке',350.00,1,350.00,'149'),(112,27,'Овощной',290.00,1,290.00,'124'),(113,28,'Сет Самурай',1200.00,1,1200.00,'135'),(114,28,'Саке',350.00,1,350.00,'149'),(115,29,'Дракон',520.00,1,520.00,'119'),(116,29,'Эби темпура',450.00,1,450.00,'121'),(117,29,'Вода без газа',90.00,1,90.00,'154'),(118,68,'Гункан с икрой',160.00,1,160.00,NULL),(119,68,'Сет Премиум',2100.00,1,2100.00,NULL),(120,68,'Моти',200.00,1,200.00,NULL),(121,68,'Пицца 4 сыра',720.00,1,720.00,NULL),(122,68,'Кола 0.5',120.00,2,240.00,NULL),(123,69,'Филадельфия классик',390.00,2,780.00,NULL),(124,69,'Вода без газа',90.00,1,90.00,NULL),(125,69,'Бонито',420.00,3,1260.00,NULL),(126,69,'Гункан спайси лосось',150.00,3,450.00,NULL),(127,69,'Дракон',520.00,7,3640.00,NULL),(128,70,'Филадельфия классик',390.00,2,780.00,NULL),(129,70,'Вода без газа',90.00,1,90.00,NULL),(130,70,'Гункан спайси лосось',150.00,3,450.00,NULL),(131,71,'Дракон',520.00,1,520.00,NULL),(132,72,'Калифорния',360.00,1,360.00,NULL),(133,73,'Суши тунец',130.00,1,130.00,NULL),(134,73,'Дракон',520.00,2,1040.00,NULL),(135,74,'Сет Острый',1150.00,1,1150.00,NULL),(136,74,'Том ям',420.00,3,1260.00,NULL),(137,75,'Калифорния',360.00,1,360.00,NULL),(138,75,'Бонито',420.00,3,1260.00,NULL),(139,76,'Калифорния',360.00,1,360.00,NULL),(140,76,'Бонито',420.00,3,1260.00,NULL),(141,77,'Калифорния',360.00,1,360.00,NULL),(142,77,'Бонито',420.00,3,1260.00,NULL),(143,78,'Филадельфия классик',390.00,3,1170.00,NULL),(144,78,'Дракон',520.00,1,520.00,NULL),(145,78,'Спайси тунец',410.00,1,410.00,NULL),(146,79,'Филадельфия классик',390.00,3,1170.00,NULL),(147,79,'Дракон',520.00,1,520.00,NULL),(148,79,'Спайси тунец',410.00,1,410.00,NULL),(149,80,'Филадельфия классик',390.00,3,1170.00,NULL),(150,80,'Дракон',520.00,1,520.00,NULL),(151,80,'Спайси тунец',410.00,1,410.00,NULL),(152,81,'Дракон',520.00,1,520.00,NULL),(153,81,'Бонито',420.00,1,420.00,NULL),(154,81,'Сяке маки',310.00,1,310.00,NULL),(155,82,'Дракон',520.00,1,520.00,NULL),(156,82,'Саке',350.00,1,350.00,NULL),(157,83,'Бонито',420.00,1,420.00,NULL);
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
  `is_deleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_statuses`
--

LOCK TABLES `order_statuses` WRITE;
/*!40000 ALTER TABLE `order_statuses` DISABLE KEYS */;
INSERT INTO `order_statuses` VALUES (1,'Готово',0),(2,'В работе',0),(3,'Отменен',1);
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
  `customer_name` varchar(255) NOT NULL,
  `phone` varchar(50) NOT NULL,
  `address` varchar(500) NOT NULL,
  `total` decimal(10,2) NOT NULL,
  `discount` decimal(10,2) DEFAULT '0.00',
  `final_total` decimal(10,2) NOT NULL,
  `order_date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `status_id` int(11) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `status_id` (`status_id`),
  CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`status_id`) REFERENCES `order_statuses` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=84 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orders`
--

LOCK TABLES `orders` WRITE;
/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
INSERT INTO `orders` VALUES (18,'Иванов Илья Алексеевич','+7(920) 101-01-01','г. Нижний Новгород, ул. Горького, д. 10',1760.00,0.00,1760.00,'2026-01-10 12:05:10',1),(19,'Петрова Анна Сергеевна','+7(920) 101-01-02','г. Балахна, ул. Комсомольская, д. 5',1050.00,0.00,1050.00,'2026-01-10 12:18:44',2),(20,'Сидоров Кирилл Андреевич','+7(920) 101-01-03','г. Дзержинск, пр-т Ленина, д. 22',2250.00,0.00,2250.00,'2026-01-10 13:02:30',1),(21,'Фомичева Ольга Викторовна','+7(920) 101-01-04','г. Заволжье, ул. Советская, д. 14',4930.00,739.50,4190.50,'2026-01-10 13:45:05',2),(22,'Смирнов Денис Олегович','+7(920) 101-01-05','г. Нижний Новгород, ул. Белинского, д. 28',850.00,0.00,850.00,'2026-01-10 14:11:19',3),(23,'Кузнецова Мария Игоревна','+7(920) 101-01-06','г. Балахна, ул. Дзержинского, д. 9',1290.00,0.00,1290.00,'2026-01-10 15:03:50',1),(24,'Волков Артем Николаевич','+7(920) 101-01-07','г. Дзержинск, ул. Маяковского, д. 3',2050.00,0.00,2050.00,'2026-01-10 15:40:12',2),(25,'Орлова Екатерина Павловна','+7(920) 101-01-08','г. Заволжье, ул. Мира, д. 6',910.00,0.00,910.00,'2026-01-11 11:09:03',1),(26,'Морозов Алексей Евгеньевич','+7(920) 101-01-09','г. Нижний Новгород, ул. Ильинская, д. 17',3250.00,0.00,3250.00,'2026-01-11 12:22:58',1),(27,'Новикова Татьяна Романовна','+7(920) 101-01-10','г. Балахна, ул. Гагарина, д. 31',640.00,0.00,640.00,'2026-01-11 12:45:27',3),(28,'Соловьев Максим Ильич','+7(920) 101-01-11','г. Дзержинск, ул. Терешковой, д. 8',1550.00,0.00,1550.00,'2026-01-11 13:15:41',1),(29,'Зайцева Полина Дмитриевна','+7(920) 101-01-12','г. Заволжье, ул. Победы, д. 12',1060.00,0.00,1060.00,'2026-01-11 14:07:09',2),(68,'Сливин Никита','+7(998) 671 42-26','Г. Балахна ул.Некрасова 42',3420.00,0.00,3420.00,'2026-01-31 11:15:08',2),(69,'Пррои Олло Лорло Лол','+7(544) 657 89-08','Авпролд34556',6220.00,933.00,5287.00,'2026-02-16 17:38:19',2),(70,'Авчпсрмоилт','+7(891) 234 56-78','Фцыукенгол',1320.00,0.00,1320.00,'2026-02-16 17:39:56',2),(71,'Еарпрррпрпрпрп','+7(333) 333 33-33','Ррррррр',520.00,0.00,520.00,'2026-02-16 17:40:50',2),(72,'Рооооооома','+7(777) 777 77-77','23232323',360.00,0.00,360.00,'2026-02-16 17:45:40',2),(73,'Ваааааася','+7(111) 111 11-11','Цацйайцайа',1170.00,0.00,1170.00,'2026-02-16 17:46:38',2),(74,'Леееееша','+7(222) 222 22-22','Аоол2ла2',2410.00,0.00,2410.00,'2026-02-16 17:47:58',2),(75,'Ааааааааааааа','+7(222) 222 22-22','12',1620.00,0.00,1620.00,'2026-02-16 17:55:22',2),(76,'Ааааааааааааа','+7(222) 222 22-22','12',1620.00,0.00,1620.00,'2026-02-16 17:55:22',2),(77,'Ааааааааааааа','+7(222) 222 22-22','12',1620.00,0.00,1620.00,'2026-02-16 17:55:22',1),(78,'Аыафыа','+7(124) 214 11-24','Афыафа',2100.00,0.00,2100.00,'2026-02-16 18:07:05',2),(79,'Аыафыа','+7(124) 214 11-24','Афыафа',2100.00,0.00,2100.00,'2026-02-16 18:07:32',2),(80,'Аыафыа','+7(124) 214 11-24','Афыафа',2100.00,0.00,2100.00,'2026-02-16 18:07:32',2),(81,'Аловпфорп','+7(114) 221 41-24','1А1а1а1а1а',1250.00,0.00,1250.00,'2026-02-16 18:09:28',2),(82,'Ауауацуа','+7(123) 112 31-23','Фвыа1а1а',870.00,0.00,870.00,'2026-02-16 18:14:21',2),(83,'Рррнрнрнр','+7(778) 787 87-88','Рпнрнооглшд',420.00,0.00,420.00,'2026-02-16 18:14:38',2);
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
  `article` varchar(50) NOT NULL,
  `name` varchar(100) NOT NULL,
  `description` text,
  `price` decimal(10,2) NOT NULL,
  `category_id` int(11) DEFAULT NULL,
  `image_path` varchar(255) DEFAULT NULL,
  `is_deleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `article` (`article`),
  KEY `idx_products_category_id` (`category_id`),
  CONSTRAINT `products_ibfk_1` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=167 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES (117,'100001','Филадельфия классик','Ролл с лососем и сливочным сыром',390.00,1,'cd886db0-ce25-4859-8e49-bd6a0404388d.jpg',0),(118,'100002','Калифорния','Ролл с крабом и авокадо',360.00,1,'e1c3f2d6-79db-4bd9-9815-146383297679.jpg',0),(119,'100003','Дракон','Ролл с угрем и авокадо',520.00,1,'e3dae24d-169e-4b0f-8868-287b3a8a642f.jpg',0),(120,'100004','Бонито','Ролл с тунцом и стружкой бонито',420.00,1,'images/100004.jpg',0),(121,'100005','Эби темпура','Теплый ролл с креветкой в кляре',450.00,1,'images/100005.jpg',0),(122,'100006','Спайси тунец','Острый ролл с тунцом',410.00,1,'c2846de8-3de7-43de-9b49-a810ad3cc405.jpg',0),(123,'100007','Спайси лосось','Острый ролл с лососем',420.00,1,'484f6778-f28c-41da-b105-8a8f9a5cdc1d.jpg',0),(124,'100008','Овощной','Ролл с огурцом, авокадо и перцем',290.00,1,'4437ab7c-c047-4dca-8f0e-ca6bfd014db0.jpg',0),(125,'100009','Унаги маки','Маки с угрем',330.00,1,'images/100009.jpg',0),(126,'100010','Сяке маки','Маки с лососем',310.00,1,'images/100010.jpg',0),(127,'100011','Суши лосось','Нигири с лососем',120.00,2,'images/100011.jpg',0),(128,'100012','Суши тунец','Нигири с тунцом',130.00,2,'images/100012.jpg',0),(129,'100013','Суши угорь','Нигири с угрем',150.00,2,'images/100013.jpg',0),(130,'100014','Суши креветка','Нигири с креветкой',125.00,2,'images/100014.jpg',0),(131,'100015','Суши краб','Нигири с крабом',115.00,2,'images/100015.jpg',0),(132,'100016','Гункан с икрой','Гункан с тобико',160.00,2,'images/100016.jpg',0),(133,'100017','Гункан спайси лосось','Гункан с острым лососем',150.00,2,'images/100017.jpg',0),(134,'100018','Гункан тунец','Гункан с тунцом',155.00,2,'images/100018.jpg',0),(135,'100019','Сет Самурай','Большой сет из 24 штук',1200.00,3,'images/100019.jpg',0),(136,'100020','Сет Токио','Сет из 18 штук',850.00,3,'images/100020.jpg',0),(137,'100021','Сет Осака','Сет из 32 штук',1550.00,3,'images/100021.jpg',0),(138,'100022','Сет Мини','Сет из 12 штук',590.00,3,'images/100022.jpg',0),(139,'100023','Сет Премиум','Сет из 40 штук',2100.00,3,'images/100023.jpg',0),(140,'100024','Сет Темпура','Сет горячих роллов 20 штук',980.00,3,'images/100024.jpg',0),(141,'100025','Сет Классика','Сет классических роллов 28 штук',1290.00,3,'images/100025.jpg',0),(142,'100026','Сет Острый','Сет острых роллов 24 штук',1150.00,3,'images/100026.jpg',0),(143,'100027','Пицца Маргарита','Томатный соус, моцарелла, базилик',550.00,4,'images/100027.jpg',0),(144,'100028','Пицца Пепперони','Пепперони, моцарелла, соус',650.00,4,'images/100028.jpg',0),(145,'100029','Пицца Ветчина-грибы','Ветчина, шампиньоны, сыр',690.00,4,'images/100029.jpg',0),(146,'100030','Пицца 4 сыра','Моцарелла, дорблю, пармезан, гауда',720.00,4,'images/100030.jpg',0),(147,'100031','Пицца Гавайская','Курица, ананас, сыр',680.00,4,'images/100031.jpg',0),(148,'100032','Пицца Барбекю','Курица, бекон, соус BBQ',740.00,4,'images/100032.jpg',0),(149,'100033','Саке','Японское рисовое вино',350.00,5,'images/100033.jpg',0),(150,'100034','Зеленый чай','Китайский зеленый чай',150.00,5,'images/100034.jpg',0),(151,'100035','Черный чай','Классический чай',130.00,5,'images/100035.jpg',0),(152,'100036','Кола 0.5','Газированный напиток',120.00,5,'images/100036.jpg',0),(153,'100037','Морс ягодный','Домашний морс',160.00,5,'images/100037.jpg',0),(154,'100038','Вода без газа','Минеральная вода 0.5',90.00,5,'images/100038.jpg',0),(155,'100039','Моти','Японский десерт с начинкой',200.00,7,'images/100039.jpg',0),(156,'100040','Чизкейк','Нежный чизкейк',220.00,7,'images/100040.jpg',0),(157,'100041','Тирамису','Классический десерт',240.00,7,'images/100041.jpg',0),(158,'100042','Панна-котта','Сливочный десерт с соусом',210.00,7,'images/100042.jpg',0),(159,'100043','Дораяки','Японские блинчики с пастой',190.00,7,'images/100043.jpg',1),(160,'100044','Вок с курицей','Лапша, курица, овощи, соус',420.00,6,'images/100044.jpg',0),(161,'100045','Вок с говядиной','Лапша, говядина, овощи, соус',460.00,6,NULL,0),(162,'100046','Вок с креветками','Лапша, креветки, овощи, соус',490.00,6,'images/100046.jpg',0),(163,'100047','Рис с овощами','Рис, овощи, соевый соус',280.00,6,'images/100047.jpg',0),(164,'100048','Рис с курицей','Рис, курица, яйцо, овощи',360.00,6,'images/100048.jpg',0),(165,'100049','Мисо суп','Мисо, тофу, водоросли',190.00,6,'images/100049.jpg',0),(166,'100050','Том ям','Острый суп с морепродуктами',420.00,6,'images/100050.jpg',0);
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
INSERT INTO `roles` VALUES (1,'Администратор'),(2,'Директор'),(3,'Продавец');
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
  `role_id` int(11) DEFAULT NULL,
  `login` varchar(20) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `login` (`login`),
  KEY `idx_users_role_id` (`role_id`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (2,'Директор Ресторана',2,'director','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b'),(3,'Менеджер Петров',3,'manager','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b'),(4,'Продавец Иванова',3,'seller1','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b'),(5,'Ванечкин Олег Иванович',3,'manager2','e0bc614e4fd035a488619799853b075143deea596c477b8dc077e309c0fe42e9'),(7,'Илья Алекснадрович',1,'ivan','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b'),(8,'Директор Василий',1,'vas','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b'),(9,'Кирилл Сливин',1,'admin1','6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b');
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

-- Dump completed on 2026-03-13  3:37:55
