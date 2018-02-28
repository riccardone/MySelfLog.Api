/// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
const uuidv5 = require('uuid/v5')
const uuidv4 = require('uuid/v4')

// Save carefully this namespace as you'll need it in the future to get the same deterministic ids
const myNamespace = 'f11c5317-06bb-47ad-b589-2b8e8332decd'

function toEventData (msg) {
  var correlationId = getCorrelationId(msg.profile)
  var applies = msg.applies
  var source = msg.source

  delete msg.profile
  delete msg.applies
  delete msg.source

  var eventData = {
    eventId: uuidv4(),
    data: msg,
    metadata: { '$correlationId': correlationId, 'Applies': applies, 'Source': source }
  }
  return eventData
}

function getCorrelationId (profile) {
  return uuidv5(profile, myNamespace)
}

module.exports.toEventData = toEventData
module.exports.getCorrelationId = getCorrelationId
