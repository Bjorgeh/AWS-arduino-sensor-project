# 📡 Kafka Setup on Banana Pi (KRaft, no Zookeeper)

This guide explains how to set up Apache Kafka (Bitnami, KRaft mode) on a Banana Pi (M4) using Docker and Docker Compose – completely without Zookeeper.

---

## 📦 Step 1: Install Docker and kafkacat

```bash
sudo apt update
sudo apt install -y docker.io docker-compose kafkacat
sudo systemctl enable docker
sudo usermod -aG docker $USER
```

> ⚠️ Log out and back in after running `usermod` to apply the group change.

---

## 📁 Step 2: Create project folder and `docker-compose.yml`

```bash
mkdir kafka-setup && cd kafka-setup
nano docker-compose.yml
```

### Contents:

```yaml
version: '3'
services:
  kafka:
    image: bitnami/kafka:3.6.1
    container_name: kafka
    ports:
      - "9092:9092"
    environment:
      - KAFKA_CFG_NODE_ID=1
      - KAFKA_CFG_PROCESS_ROLES=broker,controller
      - KAFKA_CFG_CONTROLLER_QUORUM_VOTERS=1@kafka:9093
      - KAFKA_CFG_CONTROLLER_LISTENER_NAMES=CONTROLLER
      - KAFKA_CFG_LISTENERS=INTERNAL://0.0.0.0:9092,CONTROLLER://0.0.0.0:9093
      - KAFKA_CFG_ADVERTISED_LISTENERS=INTERNAL://localhost:9092
      - KAFKA_CFG_LISTENER_SECURITY_PROTOCOL_MAP=INTERNAL:PLAINTEXT,CONTROLLER:PLAINTEXT
      - KAFKA_CFG_INTER_BROKER_LISTENER_NAME=INTERNAL
      - ALLOW_PLAINTEXT_LISTENER=yes
```

---

## 🚀 Step 3: Start Kafka

```bash
docker-compose up -d
```

---

## ✅ Step 4: Test that Kafka is running

```bash
kafkacat -b 127.0.0.1:9092 -X security.protocol=plaintext -L
```

You should see output showing `1 brokers`, `0 topics`.

---

## 🧪 Extra: Send a test message

Create and send a message to a topic named `sensor-data`:

```bash
echo "test from outside" | kafkacat -b 127.0.0.1:9092 -t sensor-data
```

---

## 🛠 Troubleshooting

* Check that the Kafka container is running:

```bash
docker ps -a
```

* View Kafka logs:

```bash
docker logs kafka | tail -n 100
```

* Restart if needed:

```bash
docker-compose down -v --remove-orphans
docker-compose up -d
```

---

## Add this to your dotnet apps (ArduinoToKakfaProducer & kafkaforwarder), they wont work without it
```dotnet add package Confluent.Kafka```

### How it Works

The Kafka Server works as a middle chain for temp data storage. Here the controller(Kafka producer) takes the arduino-sensor data and sendes it to the kafka server,
Then the KafkaForwarder gets the data from the kafka server and sends it to a Aws Lambda function - gets saved in a dynamodb table.
![image](https://github.com/user-attachments/assets/659390a9-1d24-43c4-8e64-72bff17dfdbf)

Now we can se that the new value is getting trough the system and is displayed in the UI. 
![image](https://github.com/user-attachments/assets/6f3212fc-aab8-4890-af43-4d42c8e3c0f3)

