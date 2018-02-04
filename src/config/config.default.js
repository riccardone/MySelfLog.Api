var config = module.exports = {};

config.env = 'dev';
config.host = process.env.App_Host;
config.port = process.env.App_Port;

config.projectName = "myselflog-api"; 

config.eventstoreConnection = process.env.EventStore_Link || 'tcp://eventstore:1113';
config.eventstoreConnectionSettings = process.env.Eventstore_Connection_Settings || {'admin':'changeit'};
config.publishTo = "diary-input";

config.elasticSearchLink = process.env.Elastic_Link || 'http://elasticsearch:9200';

// logging
config.logAppender = "debug";
config.logLevel = "debug";