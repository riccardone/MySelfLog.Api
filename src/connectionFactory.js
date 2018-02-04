var esClient = require('node-eventstore-client');
var cfg = require('./config');
var log4js = require('log4js');
var logger = log4js.getLogger('MessageSender');

module.exports = ConnectionFactory;

function ConnectionFactory() { }

ConnectionFactory.prototype.CreateEsConnection = function() {
    var esConnection = esClient.createConnection(cfg.eventstoreConnectionSettings, cfg.eventstoreConnection, cfg.projectName);    
    esConnection.connect();
    esConnection.once("connected", function (tcpEndPoint) {
        logger.info("Connected to eventstore at " + tcpEndPoint.host + ":" + tcpEndPoint.port)
    });    
    return esConnection;
}