var config = require('./config.default');

// config.env = 'prod';
// config.host = 'localhost';
// config.port = 3001;

// config.eventstoreConnection = 'tcp://updateme:1113';
// config.eventstoreConnectionSettings = {"updateme":"updateme"};
// config.publishTo = "diary-input";

// logging
config.logAppender = "error";
config.logLevel = "error";

module.exports = config;