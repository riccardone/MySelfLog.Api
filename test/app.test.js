let chai = require('chai');
let chaiHttp = require('chai-http');
let server = require('../src/app');
let should = chai.should();

chai.use(chaiHttp);

describe('/', () => {        
  describe('/GET data', () => {
      it('it should GET 200 (healty check)', (done) => {
        chai.request(server)
            .get('/')
            .end((err, res) => {
                res.should.have.status(200);               
              done();
            });
      });
  });
});