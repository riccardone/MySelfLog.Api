var config = module.exports = {};

config.env = 'dev';
config.host = 'localhost';
config.port = 3001;

config.projectName = "MySelfLog-Api"; // no spaces or strange chars please

config.eventstoreConnection = 'tcp://localhost:1113';
config.eventstoreConnectionSettings = {};
config.publishTo = "diary-input";

// logging
config.logAppender = "debug";
config.logLevel = "debug";