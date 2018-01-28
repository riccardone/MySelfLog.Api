var config = module.exports = {};

config.env = 'dev';
config.host = process.env.App_Host;
config.port = process.env.App_Port;

config.projectName = "MySelfLog-Api"; // no spaces or strange chars please

config.eventstoreConnection = process.env.Eventstore_Connection || 'tcp://localhost:1113';
config.eventstoreConnectionSettings = process.env.Eventstore_Connection_Settings || {};
config.publishTo = "diary-input";

config.elasticSearchLink = process.env.Elastic_Host;

// logging
config.logAppender = "debug";
config.logLevel = "debug";