package org.example;

import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.clients.producer.ProducerRecord;

import java.util.Properties;
import java.util.Random;

public class LogCompactionExample {
    public static void main(String[] args) {
        Properties producerProperties = new Properties();

        producerProperties.put("bootstrap.servers", "localhost:9092");
        producerProperties.put("client.id", "demo-producer");
        producerProperties.put("key.serializer", "org.apache.kafka.common.serialization.StringSerializer");
        producerProperties.put("value.serializer", "org.apache.kafka.common.serialization.StringSerializer");

        final Producer<String, String> producer = new KafkaProducer<>(producerProperties);
        final String chars = "ABC";
        Random random = new Random();

        // value depends on the segment configuration --> was tested with docker test configuration
        final String heavyPayload = "loremipsum".repeat(50000);

        for (int i = 0; i < 1000; i++) {
            ProducerRecord<String, String> secondRecord = new ProducerRecord<>("my-compacted-topic", Character.toString(chars.charAt(random.nextInt(chars.length()))), String.format("%d %s", i, heavyPayload));

            producer.send(secondRecord, (metadata, exception) -> {
                if (exception != null) {
                    System.out.println(exception.getMessage());
                }
            });
        }

        producer.flush();
        producer.close();
    }
}
