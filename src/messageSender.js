var log4js = require('log4js');
var logger = log4js.getLogger('MessageSender');
var eventDataMapper = require('./eventdata-mapper');
var cfg = require('./config');
var esClient = require('node-eventstore-client');
var _esConnection;

function MessageSender(conn) {
    this._esConnection = conn;
}

MessageSender.prototype.send = function (message, eventType) {
    var eventData = eventDataMapper.toEventData(message);
    var eventId = eventData.eventId;
    var event = esClient.createJsonEventData(eventId, eventData.data, eventData.metadata, eventType);
    return this._esConnection.appendToStream(cfg.publishTo, esClient.expectedVersion.any, event);
}

module.exports = MessageSender;