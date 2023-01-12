package com.example.springkafkademo;

import org.apache.kafka.clients.admin.NewTopic;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.kafka.config.TopicBuilder;
import org.springframework.scheduling.annotation.EnableScheduling;

@SpringBootApplication
@EnableScheduling
public class SpringKafkaDemoApplication {
	Logger logger = LoggerFactory.getLogger(SpringKafkaDemoApplication.class);

	public static void main(String[] args) {
		SpringApplication.run(SpringKafkaDemoApplication.class, args);
	}

	@Bean
	public NewTopic topic() {
		return TopicBuilder.name("my-spring-topic")
				.partitions(1)
				.replicas(1)
				.build();
	}

	@KafkaListener(id = "myId", topics = "my-spring-topic")
	public void listen(String consumedMessage) {
		logger.info(consumedMessage);
	}
}