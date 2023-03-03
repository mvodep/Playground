package org.example;

import io.lettuce.core.RedisClient;
import io.lettuce.core.api.StatefulRedisConnection;
import io.lettuce.core.api.sync.RedisStreamCommands;
import org.apache.kafka.common.utils.AppInfoParser;
import org.apache.kafka.connect.data.Schema;
import org.apache.kafka.connect.data.Struct;
import org.apache.kafka.connect.sink.SinkRecord;
import org.apache.kafka.connect.sink.SinkTask;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.Collection;
import java.util.Collections;
import java.util.Map;

import static org.apache.kafka.connect.transforms.util.Requirements.requireStruct;

public class SimpleRedisSinkTask extends SinkTask {
    private static final Logger log = LoggerFactory.getLogger(SimpleRedisSinkTask.class);
    private StatefulRedisConnection<String, String> connection;
    private RedisClient client;
    private RedisStreamCommands<String, String> syncCommands;

    private Schema keySchema;

    private static final String PURPOSE = "Get values of SinkRecord";

    @Override
    public String version() {
        return AppInfoParser.getVersion();
    }

    @Override
    public void start(Map<String, String> props) {
        client = RedisClient.create("redis://redis:6379");
        connection = client.connect();
        syncCommands = connection.sync();
    }

    @Override
    public void put(Collection<SinkRecord> records) {
        for (SinkRecord record : records) {
            final Struct value = requireStruct(record.value(), PURPOSE);

            Map<String, String> body = Collections.singletonMap("value", value.get("value").toString());

            syncCommands.xadd(record.key().toString(), body);
        }
    }

    @Override
    public void stop() {
        connection.close();
        client.shutdown();
    }
}
