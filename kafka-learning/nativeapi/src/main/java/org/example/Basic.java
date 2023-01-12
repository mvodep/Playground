package org.example;

import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.clients.consumer.ConsumerRecords;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.clients.producer.*;

import java.time.Duration;
import java.util.Collections;
import java.util.Properties;
import java.util.UUID;

public class Basic {
    public static void main(String[] args) {
        Properties producerProperties = new Properties();

        // You can also use "ProducerConfig.BOOTSTRAP_SERVERS_CONFIG" if preferred
        producerProperties.put("bootstrap.servers", "localhost:9092");
        producerProperties.put("client.id", "demo-producer");
        producerProperties.put("key.serializer", "org.apache.kafka.common.serialization.StringSerializer");
        producerProperties.put("value.serializer", "org.apache.kafka.common.serialization.StringSerializer");

        final Producer<String, String> producer = new KafkaProducer<>(producerProperties);

        ProducerRecord<String, String> producerRecord = new ProducerRecord<>("my-events", "key1", "hello world");

        try {
            // we are using Future.get() to wait for a reply from Kafka
            RecordMetadata metadata = producer.send(producerRecord).get();

            System.out.printf("Topic: %s\nPartition: %s\nOffset: %s\nTimestamp: %s%n",
                    metadata.topic(), metadata.partition(), metadata.offset(), metadata.timestamp());

        } catch (Exception e) {
            e.printStackTrace();
        }

        // To gain more speed we can also send records asynchronously
        ProducerRecord<String, String> secondRecord = new ProducerRecord<>("my-events", "key2", "lorem ipsum");

        producer.send(secondRecord, (metadata, exception) -> {
            if (exception == null) {
                System.out.printf("Topic: %s\nPartition: %s\nOffset: %s\nTimestamp: %s%n",
                        metadata.topic(), metadata.partition(), metadata.offset(), metadata.timestamp());
            } else {
                System.out.println(exception.getMessage());
            }
        });

        producer.flush();
        producer.close();

        // ****** Consume ******
        Properties consumerProperties = new Properties();

        consumerProperties.put("bootstrap.servers", "localhost:9092");
        // we generate a new group id every time. Otherwise, it can block until dead consumers time out
        consumerProperties.put("group.id", UUID.randomUUID().toString());
        consumerProperties.put("auto.offset.reset", "earliest");
        consumerProperties.put("enable.auto.commit", "false");
        consumerProperties.put("key.deserializer", "org.apache.kafka.common.serialization.StringDeserializer");
        consumerProperties.put("value.deserializer", "org.apache.kafka.common.serialization.StringDeserializer");

        KafkaConsumer<String, String> consumer = new KafkaConsumer<>(consumerProperties);

        consumer.subscribe(Collections.singletonList("my-events"));

        Duration timeout = Duration.ofMillis(100);
        while (true) {
            ConsumerRecords<String, String> consumerRecords = consumer.poll(timeout);
            for (ConsumerRecord<String, String> record : consumerRecords) {
                System.out.printf("consumed topic = %s, partition = %d, offset = %d, " +
                                "key = %s, value = %s\n",
                        record.topic(), record.partition(), record.offset(),
                        record.key(), record.value());
            }
        }
    }
}