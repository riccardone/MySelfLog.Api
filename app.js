require('dotenv').config()
var publishTo = 'diary-input'
// logger setup
var log4js = require('log4js')
var loggerConfig = require('./src/logging/')
loggerConfig.run()
var logger = log4js.getLogger('myselflog-api')
var service = require('./src/service')({publishTo: publishTo, logger: logger})

var server = service.listen(process.env.App_Port, '0.0.0.0', function () {
  var address = server.address().address + ':' + server.address().port

  logger.info('listening on %s', address)
  logger.info('publishing to %s', publishTo)  
})

module.exports = server
