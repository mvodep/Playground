package org.example;

import org.apache.kafka.common.config.ConfigDef;
import org.apache.kafka.connect.connector.ConnectRecord;
import org.apache.kafka.connect.data.Struct;
import org.apache.kafka.connect.transforms.Transformation;

import java.util.Map;

import static org.apache.kafka.connect.transforms.util.Requirements.requireStruct;

public class MeasurementValueToKey<R extends ConnectRecord<R>> implements Transformation<R> {

    public static final String OVERVIEW_DOC = "Copy the id value to the key";
    private static final String PURPOSE = "Copy the id value to the key";

    public static final ConfigDef CONFIG_DEF = new ConfigDef();

    @Override
    public R apply(R record) {
        return applyWithSchema(record);
    }

    @Override
    public ConfigDef config() {
        return CONFIG_DEF;
    }

    @Override
    public void close() {

    }

    @Override
    public void configure(Map<String, ?> configs) {

    }

    private R applyWithSchema(R record) {
        final Struct value = requireStruct(record.value(), PURPOSE);
        final Struct idStruct = requireStruct(value.get("id"), PURPOSE);

        final String key = String.format("urn:sensor:%s:%d", idStruct.get("location").toString(), Integer.parseInt(idStruct.get("id").toString()));

        return record.newRecord(record.topic(), record.kafkaPartition(), null, key, value.schema(), value, record.timestamp());    }
}