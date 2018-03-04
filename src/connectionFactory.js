module.exports = function (logger) {  
  var esClient = require('node-eventstore-client')
  var _logger = logger

  var createEsConnection = () => {
    var esConnection = esClient.createConnection({ 'admin': 'changeit' }, 'tcp://eventstore:1113', 'myselflog-api')
    esConnection.on('error', err =>
      _logger.error(`Error occurred on connection: ${err}`)
    )
    esConnection.on('closed', (reason) => {
      _logger.error(`Connection closed, reason: ${reason}`)
      _logger.error('dying...')
      process.exit(1)
    })
    esConnection.once('connected', (tcpEndPoint) => {
      _logger.info('Connected to eventstore at ' + tcpEndPoint.host + ':' + tcpEndPoint.port)
    })
    esConnection.connect().catch(err => _logger.error(err))
    return esConnection
  }

  return {CreateEsConnection: createEsConnection}
}
