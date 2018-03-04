/* global describe it */
let chai = require('chai')
let chaiHttp = require('chai-http')
var sinon = require('sinon')

let should = chai.should()

chai.use(chaiHttp)

describe('/GET data', () => {
  it('Healty-check It should GET 200', (done) => {
    chai.request(buildService())
      .get('/')
      .end((err, res) => {
        should.not.exist(err)
        res.should.have.status(200)
        done()
      })
  })
})

describe('/GET Diary', () => {
  it('Given existant diary name It should get the Diary', (done) => {
    var FakeEsClient = sinon.stub()
    FakeEsClient.prototype.get = sinon.stub().resolves({ found: true, _source: 'test' })
    chai.request(buildService(null, new FakeEsClient()))
      .get('/api/v1/diary/ciccio')
      .end((err, res) => {
        should.not.exist(err)
        res.should.have.status(200)
        done()
      })
  })
  it('Given non-existant diary name It should NOT get the Diary', (done) => {
    var FakeEsClient = sinon.stub()
    FakeEsClient.prototype.get = sinon.stub().resolves({ found: false, _source: 'test' })
    chai.request(buildService(null, new FakeEsClient()))
      .get('/api/v1/diary/ciccio')
      .end((err, res) => {
        err.should.have.status(404)
        done()
      })
  })
})

describe('Check if Diary exist', () => {
  it('Given existant diary name It should get NOT AVAILABLE', (done) => {
    var FakeClient = sinon.stub()
    FakeClient.prototype.search = sinon.stub().resolves({ hits: { total: 1 } })
    chai.request(buildService(null, new FakeClient()))
      .get('/api/v1/diary/check/ciccio')
      .end((err, res) => {
        should.not.exist(err)
        res.should.have.status(200)
        res.should.have.property('text', 'not available')
        done()
      })
  })
  it('Given non-existant diary name It should get AVAILABLE', (done) => {
    var FakeClient = sinon.stub()
    FakeClient.prototype.search = sinon.stub().resolves({ hits: { total: 0 } })
    chai.request(buildService(null, new FakeClient()))
      .get('/api/v1/diary/check/ciccio')
      .end((err, res) => {
        should.not.exist(err)
        res.should.have.status(200)
        res.should.have.property('text', 'available')
        done()
      })
  })
})

describe('/POST /api/v1/diary', () => {
  describe('/POST wrong data', () => {
    it('it should POST 400 (Bad Request)', (done) => {
      chai.request(buildService())
        .post('/api/v1/diary')
        .end((err, res) => {
          err.should.have.status(400)
          done()
        })
    })
  })
})

function buildService (sender, esClient) {
  if (!sender) {
    var SenderModule = require('../src/messageSender')
    sender = new SenderModule(null, 'test')
  }
  const fakeLogger = sinon.stub()
  fakeLogger.prototype.error = sinon.stub()
  fakeLogger.prototype.info = sinon.stub()
  if (!esClient) {
    var FakeEsClient = sinon.stub()
    esClient = new FakeEsClient()
  }
  return require('../src/service')({ sender: sender, esClient: esClient, logger: fakeLogger })
}
