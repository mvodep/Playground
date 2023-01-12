package com.example.springkafkademo;

import org.apache.kafka.clients.producer.RecordMetadata;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.kafka.core.KafkaTemplate;
import org.springframework.kafka.support.SendResult;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.concurrent.CompletableFuture;

@Service
public class PeriodicTrainingProducer {
    Logger logger = LoggerFactory.getLogger(PeriodicTrainingProducer.class);

    private final KafkaTemplate<String, String> kafkaTemplate;
    private WebClient client = WebClient.create("https://api.quotable.io");

    public PeriodicTrainingProducer(KafkaTemplate<String, String> avroKafkaTemplate) {
        this.kafkaTemplate = avroKafkaTemplate;
    }

    @Scheduled(fixedRate = 3000)
    public void publishQuote() {
        String randomQuoteResponseMono = this.client.get().uri("random").retrieve().bodyToMono(String.class).block();

        CompletableFuture<SendResult<String, String>> future = kafkaTemplate.send("my-spring-topic", randomQuoteResponseMono);

        future.whenComplete((result, ex) -> {
            if (ex == null) {
                RecordMetadata metadata = result.getRecordMetadata();

                logger.info(String.format("Topic: %s\nPartition: %s\nOffset: %s\nTimestamp: %s%n",
                        metadata.topic(), metadata.partition(), metadata.offset(), metadata.timestamp()));
            } else {
                logger.error(ex.getMessage());
            }
        });
    }
}
