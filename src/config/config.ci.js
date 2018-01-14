var config = require('./config.default');

config.env = 'ci';
config.host = 'localhost';
config.port = 3001;

config.eventstoreConnection = 'notneeded-for-CI';
config.eventstoreConnectionSettings = {};
config.publishTo = "notneeded-for-CI";

// logging
config.logAppender = "ci";
config.logLevel = "info";

module.exports = config;