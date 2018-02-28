var eventDataMapper = require('./eventdata-mapper')
var esClient = require('node-eventstore-client')
var _esConnection
var _publishTo = 'diary-input'

function MessageSender (conn, publishTo) {
  this._esConnection = conn
  if (publishTo) {
    _publishTo = publishTo
  }
}

MessageSender.prototype.send = function (message, eventType) {
  var eventData = eventDataMapper.toEventData(message)
  var eventId = eventData.eventId
  var event = esClient.createJsonEventData(eventId, eventData.data, eventData.metadata, eventType)
  return _esConnection.appendToStream(_publishTo, esClient.expectedVersion.any, event)
}

module.exports = MessageSender
