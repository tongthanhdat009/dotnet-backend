CREATE TABLE `categories` (
  `category_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `category_name` varchar(100) NOT NULL
);

CREATE TABLE `suppliers` (
  `supplier_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `phone` varchar(20) DEFAULT null,
  `email` varchar(100) DEFAULT null,
  `address` text
);

CREATE TABLE `customers` (
  `customer_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `phone` varchar(20) DEFAULT null,
  `email` varchar(100) DEFAULT null,
  `password` varchar(255) DEFAULT null,
  `address` text,
  `created_at` timestamp DEFAULT (now())
);

CREATE TABLE `products` (
  `product_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `category_id` int DEFAULT null,
  `supplier_id` int DEFAULT null,
  `product_name` varchar(100) NOT NULL,
  `barcode` varchar(50) DEFAULT null,
  `price` decimal(10,2) NOT NULL,
  `unit` varchar(20) DEFAULT 'pcs',
  `deleted` TINYINT(1) DEFAULT 0, -- Cột mới: Trạng thái xóa mềm (0: chưa xóa, 1: đã xóa)
  `image_url` varchar(255) DEFAULT null,
  `created_at` timestamp DEFAULT (now())
);

CREATE TABLE `inventory` (
  `inventory_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `quantity` int DEFAULT '0',
  `updated_at` timestamp DEFAULT (now())
);

CREATE TABLE `promotions` (
  `promo_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `promo_code` varchar(50) NOT NULL,
  `description` varchar(255) DEFAULT null,
  `discount_type` ENUM ('percent', 'fixed') NOT NULL,
  `discount_value` decimal(10,2) NOT NULL,
  `start_date` date NOT NULL,
  `end_date` date NOT NULL,
  `min_order_amount` decimal(10,2) DEFAULT '0.00',
  `usage_limit` int DEFAULT '0',
  `used_count` int DEFAULT '0',
  `status` ENUM ('active', 'inactive') DEFAULT 'active'
);

CREATE TABLE `roles` (
  `role_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `role_name` varchar(50) NOT NULL,
  `description` text
);

CREATE TABLE `users` (
  `user_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `password` varchar(255) NOT NULL,
  `full_name` varchar(100) DEFAULT null,
  `role` int DEFAULT null,
  `created_at` timestamp DEFAULT (now())
);

CREATE TABLE `orders` (
  `order_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `customer_id` int DEFAULT null,
  `user_id` int DEFAULT null,
  `promo_id` int DEFAULT null,
  `order_date` timestamp DEFAULT (now()),
  
  -- Cột mới: Trạng thái thanh toán
  `pay_status` ENUM ('pending', 'paid', 'canceled', 'refunded') DEFAULT 'pending',
  
  -- Cột mới: Trạng thái vận hành đơn hàng
  `order_status` ENUM ('pending', 'approved', 'processing', 'shipping', 'delivered', 'completed', 'canceled') DEFAULT 'pending',
  
  `total_amount` decimal(10,2) DEFAULT null,
  `discount_amount` decimal(10,2) DEFAULT '0.00',
  `order_type` ENUM ('online', 'offline') DEFAULT 'offline',
  
  -- Thông tin giao hàng
  `name` varchar(100) DEFAULT null,
  `address` text DEFAULT null,
  `phone` varchar(20) DEFAULT null,
  `email` varchar(100) DEFAULT null
);

CREATE TABLE `order_items` (
  `order_item_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `order_id` int DEFAULT null,
  `product_id` int DEFAULT null,
  `quantity` int NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `subtotal` decimal(10,2) NOT NULL
);

CREATE TABLE `payments` (
  `payment_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `amount` decimal(10,2) NOT NULL,
  `payment_method` ENUM ('cash', 'card', 'bank_transfer', 'e-wallet') DEFAULT 'cash',
  
  -- Cột mới: Trạng thái giao dịch (cho cổng thanh toán)
  `transaction_status` ENUM ('pending', 'success', 'failed') DEFAULT 'pending',
  
  `payment_date` timestamp DEFAULT (now())
);

CREATE TABLE `permissions` (
  `permission_id` int PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `permission_name` varchar(100) NOT NULL,
  `action_key` varchar(50) NOT NULL,
  `description` text
);

CREATE TABLE `role_permissions` (
  `role_id` int NOT NULL,
  `permission_id` int NOT NULL,
  PRIMARY KEY (`role_id`, `permission_id`)
);

CREATE TABLE `cart_items` (
  `product_id` INT NOT NULL,
  `customer_id` INT NOT NULL,
  `quantity` INT NOT NULL DEFAULT 1,
  `price` DECIMAL(10,2) NOT NULL,
  `subtotal` DECIMAL(10,2) NOT NULL,
  `added_at` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `bills` (
  `bill_id` INT PRIMARY KEY AUTO_INCREMENT,
  `order_id` INT NOT NULL,
  `customer_id` INT DEFAULT NULL,
  `total_amount` DECIMAL(10,2) NOT NULL,
  `discount_amount` DECIMAL(10,2) DEFAULT NULL,
  `final_amount` DECIMAL(10,2) NOT NULL,
  `payment_method` VARCHAR(50) DEFAULT NULL,
  
  -- Cột sửa đổi: Trạng thái tiền của hóa đơn
  `pay_status` ENUM('unpaid', 'paid', 'refunded') DEFAULT 'unpaid',
  
  -- Cột mới: Trạng thái xử lý hóa đơn
  `bill_status` ENUM('pending', 'exported', 'canceled') DEFAULT 'pending',
  
  `created_at` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  `paid_at` TIMESTAMP DEFAULT NULL,
  
  -- Thông tin giao hàng
  `name` varchar(100) DEFAULT null,
  `address` text DEFAULT null,
  `phone` varchar(20) DEFAULT null,
  `email` varchar(100) DEFAULT null
);

-- Indexes
CREATE UNIQUE INDEX `email` ON `customers` (`email`);
CREATE UNIQUE INDEX `barcode` ON `products` (`barcode`);
CREATE INDEX `fk_products_categories` ON `products` (`category_id`);
CREATE INDEX `fk_products_suppliers` ON `products` (`supplier_id`);
CREATE INDEX `fk_inventory_products` ON `inventory` (`product_id`);
CREATE UNIQUE INDEX `promo_code` ON `promotions` (`promo_code`);
CREATE UNIQUE INDEX `role_name` ON `roles` (`role_name`);
CREATE UNIQUE INDEX `username` ON `users` (`username`);
CREATE INDEX `role` ON `users` (`role`);
CREATE INDEX `fk_orders_customers` ON `orders` (`customer_id`);
CREATE INDEX `fk_orders_users` ON `orders` (`user_id`);
CREATE INDEX `fk_orders_promotions` ON `orders` (`promo_id`);
CREATE INDEX `fk_order_items_orders` ON `order_items` (`order_id`);
CREATE INDEX `fk_order_items_products` ON `order_items` (`product_id`);
CREATE INDEX `fk_payments_orders` ON `payments` (`order_id`);
CREATE UNIQUE INDEX `action_key` ON `permissions` (`action_key`);
CREATE INDEX `permission_id` ON `role_permissions` (`permission_id`);
CREATE INDEX `fk_cart_items_products` ON `cart_items` (`product_id`);
CREATE INDEX `fk_cart_items_customers` ON `cart_items` (`customer_id`);
CREATE INDEX `fk_bills_orders` ON `bills` (`order_id`);
CREATE INDEX `fk_bills_customers` ON `bills` (`customer_id`);

-- Foreign Keys
ALTER TABLE `products` ADD CONSTRAINT `fk_products_categories` FOREIGN KEY (`category_id`) REFERENCES `categories` (`category_id`) ON DELETE SET NULL;
ALTER TABLE `products` ADD CONSTRAINT `fk_products_suppliers` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`supplier_id`) ON DELETE SET NULL;
ALTER TABLE `inventory` ADD CONSTRAINT `fk_inventory_products` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE CASCADE;
ALTER TABLE `users` ADD CONSTRAINT `users_ibfk_1` FOREIGN KEY (`role`) REFERENCES `roles` (`role_id`);
ALTER TABLE `orders` ADD CONSTRAINT `fk_orders_customers` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`customer_id`) ON DELETE SET NULL;
ALTER TABLE `orders` ADD CONSTRAINT `fk_orders_promotions` FOREIGN KEY (`promo_id`) REFERENCES `promotions` (`promo_id`) ON DELETE SET NULL;
ALTER TABLE `orders` ADD CONSTRAINT `fk_orders_users` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE SET NULL;
ALTER TABLE `order_items` ADD CONSTRAINT `fk_order_items_orders` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE CASCADE;
ALTER TABLE `order_items` ADD CONSTRAINT `fk_order_items_products` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE RESTRICT;
ALTER TABLE `payments` ADD CONSTRAINT `fk_payments_orders` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE CASCADE;
ALTER TABLE `role_permissions` ADD CONSTRAINT `role_permissions_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE CASCADE;
ALTER TABLE `role_permissions` ADD CONSTRAINT `role_permissions_ibfk_2` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`permission_id`) ON DELETE CASCADE;
ALTER TABLE `cart_items` ADD CONSTRAINT `fk_cart_items_customers` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`customer_id`) ON DELETE CASCADE;
ALTER TABLE `cart_items` ADD CONSTRAINT `fk_cart_items_products` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE CASCADE;
ALTER TABLE `bills` ADD CONSTRAINT `fk_bills_orders` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE CASCADE;
ALTER TABLE `bills` ADD CONSTRAINT `fk_bills_customers` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`customer_id`) ON DELETE SET NULL;