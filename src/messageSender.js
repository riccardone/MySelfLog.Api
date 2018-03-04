module.exports = function (conn, publishTo) {
  var eventDataMapper = require('./eventdata-mapper')
  var esClient = require('node-eventstore-client')
  var _esConnection = conn
  var _publishTo = publishTo || 'diary-input'

  var send = function (message, eventType) {
    var eventData = eventDataMapper.toEventData(message)
    var eventId = eventData.eventId
    var event = esClient.createJsonEventData(eventId, eventData.data, eventData.metadata, eventType)
    return _esConnection.appendToStream(_publishTo, esClient.expectedVersion.any, event)
  }

  return {send: send}
}
