var esClient = require('node-eventstore-client');
var log4js = require('log4js');
var logger = log4js.getLogger('MessageSender');

module.exports = ConnectionFactory;

function ConnectionFactory() { }

ConnectionFactory.prototype.CreateEsConnection = function () {
    var esConnection = esClient.createConnection({'admin':'changeit'}, 'tcp://eventstore:1113', 'myselflog-api');
    esConnection.on("error", err =>
        logger.error(`Error occurred on connection: ${err}`)
    );
    esConnection.on("closed", reason =>
        logger.warn(`Connection closed, reason: ${reason}`)
    );
    esConnection.once("connected", function (tcpEndPoint) {
        logger.info("Connected to eventstore at " + tcpEndPoint.host + ":" + tcpEndPoint.port)
    });
    esConnection.connect().catch(err => console.log(err));    
    return esConnection;
}