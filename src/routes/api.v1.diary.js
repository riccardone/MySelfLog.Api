module.exports = function (sender, elasticClient, logger) {
  var eventDataMapper = require('../eventdata-mapper')
  var validation = require('../validation')
  const express = require('express')
  var router = express.Router()
  var _sender = sender
  var _esClient = elasticClient
  var _logger = logger

  router.get('/check/:diaryName', function (req, res) {
    // Query elastic and check if this diaryName is already in use
    var diaryName = req.params.diaryName
    _esClient.search({
      index: 'diary-events',
      q: 'DiaryName:"' + diaryName + '"'
    }, function (error, response) {
      if (error) {
        if (error.message === 'Not Found') {
          return res.status(404).send(error.message)
        }
        _logger.error(error.message)
        return res.status(500).send(error.message)
      }
      if (response.hits.total > 0) {
        return res.status(200).send('not available')
      }
      return res.status(200).send('available')
    })
  })

  router.get('/:profile', function (req, res) {
    var id = eventDataMapper.getCorrelationId(req.params.profile)
    // Query elastic and check if this diaryName is already in use
    _esClient.get({
      index: 'diary-events',
      type: 'diaryEvent',
      id: id
    }, function (error, response) {
      if (error) {
        if (error.message === 'Not Found') {
          return res.status(404).send(error.message)
        }
        _logger.error(error.message)
        return res.status(500).send(error.message)
      }
      return res.status(200).send(response._source)
    })
  })

  router.post('/', function (req, res) {
    if (validation.requestNotValid(req)) {
      return res.status(400).send('Request body not valid')
    }
    // TODO change this nonsense event type and use a proper one
    _sender.send(req.body, 'CreateDiary').then(function (result) {
      res.send() // TODO redirect on the diary?
    }).catch((error) => {
      _logger.error(error.message || error)
      res.status(500).send('There is a technical problem and the log has not been stored')
    })
  })

  return router
}
