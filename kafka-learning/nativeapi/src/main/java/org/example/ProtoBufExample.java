package org.example;

import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.Producer;
import org.apache.kafka.clients.producer.ProducerRecord;
import org.apache.kafka.clients.producer.RecordMetadata;
import org.example.proto.Person;

import java.util.Properties;
public class ProtoBufExample {
    public static void main(String[] args) {
        Properties producerProperties = new Properties();

        producerProperties.put("bootstrap.servers", "localhost:9092");
        producerProperties.put("client.id", "demo-producer");
        producerProperties.put("key.serializer", "org.apache.kafka.common.serialization.StringSerializer");
        producerProperties.put("value.serializer", "io.confluent.kafka.serializers.protobuf.KafkaProtobufSerializer");
        producerProperties.put("schema.registry.url", "http://127.0.0.1:8081");
        producerProperties.put("auto.register.schemas", "true");

        final Producer<String, Person> producer = new KafkaProducer<>(producerProperties);

        Person john = Person.newBuilder().setId(1234).setName("John Doe").setEmail("jdoe@example.com")
                .addPhones(Person.PhoneNumber.newBuilder().setNumber("555-4321").setType(Person.PhoneType.HOME))
                .build();

        ProducerRecord<String, Person> record = new ProducerRecord<>("proto-events", "a", john);

        try {
            RecordMetadata metadata = producer.send(record).get();

            System.out.printf("Topic: %s\nPartition: %s\nOffset: %s\nTimestamp: %s%n",
                    metadata.topic(), metadata.partition(), metadata.offset(), metadata.timestamp());
        } catch (Exception e) {
            e.printStackTrace();
        }

        producer.flush();
        producer.close();
    }
}
