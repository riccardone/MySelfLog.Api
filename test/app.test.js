let chai = require('chai');
let chaiHttp = require('chai-http');
var sinon = require('sinon')
//let server = require('../src/app');
let serviceModule = require('../src/service');
let senderModule = require('../src/messageSender');
let should = chai.should();

chai.use(chaiHttp);

var sender = new senderModule(null, "test");
const fakeEsClient = sinon.stub();
var esClient = new fakeEsClient();
const fakeLogger = sinon.stub();
fakeLogger.prototype.error = sinon.stub();
fakeLogger.prototype.info = sinon.stub();
var service = new serviceModule(sender, esClient, new fakeLogger());

describe('/', () => {
  describe('/GET data', () => {
    it('it should GET 200 (healty check)', (done) => {
      chai.request(service.server)
        .get('/')
        .end((err, res) => {
          res.should.have.status(200);
          done();
        });
    });
    it('it should GET 200 (diary found)', (done) => {
      fakeEsClient.prototype.get = sinon.stub().resolves({ found: true, _source: "test" });
      chai.request(service.server)
        .get('/api/v1/diary/ciccio')
        .end((err, res) => {
          res.should.have.status(200);
          done();
        });
    });
    it('it should GET 404 (diary not found)', (done) => {
      fakeEsClient.prototype.get = sinon.stub().resolves({ found: false, _source: "test" });
      chai.request(service.server)
        .get('/api/v1/diary/ciccio')
        .end((err, res) => {
          res.should.have.status(404);
          done();
        });
    });
  });
});
describe('/POST /api/v1/diary', () => {
  describe('/POST wrong data', () => {
    it('it should POST 400 (Bad Request)', (done) => {
      chai.request(service.server)
        .post('/api/v1/diary')
        .end((err, res) => {
          res.should.have.status(400);
          done();
        });
    });
  });
});