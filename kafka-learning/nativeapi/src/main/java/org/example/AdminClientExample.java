package org.example;

import org.apache.kafka.clients.admin.*;
import org.apache.kafka.common.config.ConfigResource;
import org.apache.kafka.common.config.TopicConfig;

import java.util.*;
import java.util.concurrent.ExecutionException;

public class AdminClientExample {
    public static void main(String[] args) {
        Properties adminClientProperties = new Properties();

        adminClientProperties.put(AdminClientConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        adminClientProperties.put(AdminClientConfig.CLIENT_ID_CONFIG, "admin-client");

        try (final AdminClient adminClient = AdminClient.create(adminClientProperties)) {

            ListTopicsResult listTopicsResult = adminClient.listTopics();
            Set<String> existingTopicNames = listTopicsResult.names().get();

            System.out.printf("The following topics already exist: %s", String.join("\n", existingTopicNames));

            String topicName = "example-topic";

            if (!existingTopicNames.contains(topicName)) {
                var exampleTopic = new NewTopic(topicName, 3, (short) 1);

                Map<String, String> topicConfigs = new HashMap<>();
                topicConfigs.put(TopicConfig.CLEANUP_POLICY_CONFIG, TopicConfig.CLEANUP_POLICY_DELETE);
                topicConfigs.put(TopicConfig.RETENTION_MS_CONFIG, "60000");

                exampleTopic.configs(topicConfigs);

                CreateTopicsResult topicResult = adminClient.createTopics(Collections.singletonList(exampleTopic));
                Config config = topicResult.config(topicName).get();
                System.out.printf("Topic %s created with the following configs %s%n", topicName, config.entries());
            } else {
                System.out.println("Topic already exists");
                AlterConfigOp cleanupConfig = new AlterConfigOp(
                        new ConfigEntry(TopicConfig.CLEANUP_POLICY_CONFIG, TopicConfig.CLEANUP_POLICY_COMPACT),
                        AlterConfigOp.OpType.SET);
                AlterConfigOp retentionTimeConfig = new AlterConfigOp(
                        new ConfigEntry(TopicConfig.RETENTION_MS_CONFIG, null),
                        AlterConfigOp.OpType.DELETE);

                ConfigResource topicResource = new ConfigResource(ConfigResource.Type.TOPIC, topicName);
                Map<ConfigResource, Collection<AlterConfigOp>> alterConifgMap = new HashMap<>();
                alterConifgMap.put(topicResource, Arrays.asList(cleanupConfig, retentionTimeConfig));
                AlterConfigsResult alterConfigsResult = adminClient.incrementalAlterConfigs(alterConifgMap);
                alterConfigsResult.all().get();

                System.out.println("Altered Topic Configuration");
            }
        } catch (InterruptedException | ExecutionException e) {
            e.printStackTrace();
        }
    }
}
