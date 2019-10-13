module.exports = function (sender, elasticClient, logger) {
  var validation = require('../validation')
  const express = require('express')
  var router = express.Router()
  var _sender = sender
  var _logger = logger

  router.post('/', function (req, res) {
    if (validation.requestNotValid(req)) {
      var errorMessage = 'Request body not valid'
      return res.status(400).send(errorMessage)
    }
    _sender.send(req.body, 'LogReceived').then(function (result) {
      res.send()
    }).catch((error) => {
      _logger.error(error)
      res.status(500).send('There is a technical problem and the log has not been stored')
    })
  })

  return router
}
