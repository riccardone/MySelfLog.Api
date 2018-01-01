var config = require('./config.default');

config.env = 'test';
config.host = 'localhost';
config.port = 3001;

config.eventstoreConnection = 'tcp://eventstoretest:1113';
config.eventstoreConnectionSettings = {"admin":"changeit"};
config.publishTo = "diary-input";

// logging
config.logAppender = "info";
config.logLevel = "info";

module.exports = config;