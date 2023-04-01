/*!

=========================================================
* Argon Dashboard React - v1.0.0
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-react
* Copyright 2019 Creative Tim (https://www.creative-tim.com)
* Licensed under MIT (https://github.com/creativetimofficial/argon-dashboard-react/blob/master/LICENSE.md)

* Coded by Creative Tim

=========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

*/
import { useEffect, useState } from "react";
import { connect } from 'react-redux';
import { Route, Switch, Redirect } from "react-router-dom";
// reactstrap components
import { Container, Row, Col } from "reactstrap";
import { Oval } from 'react-loader-spinner'

// core components
import AuthNavbar from "../components/Navbars/AuthNavbar.jsx";
import AuthFooter from "../components/Footers/AuthFooter.jsx";
import { hasRegistered } from "../store/actions/UserActions"
import { validateLogin } from "../store/actions/UserActions"

import routes from "../routes.js";
import requestrrLogo from "../assets/img/brand/requestrr.svg";

function Auth(props) {
  const [isLoading, setIsLoading] = useState(true);


  useEffect(() => {
    document.body.classList.add("bg-default");

    props.validateRegistration()
      .then(data => {
        props.validateLogin()
          .then(data => setIsLoading(false));
      });

    return () => {
      document.body.classList.remove("bg-default");
    }
  }, []);




  const getRoutes = (routes, path) => {
    return routes.map((prop, key) => {
      if (prop.layout === "/auth" && prop.path === path) {
        return (
          <Route
            path={prop.layout + prop.path}
            children={prop.component}
            key={key}
          />
        );
      } else {
        return null;
      }
    });
  };



  return (
    <>
      <div className="main-content">
        <AuthNavbar />
        <div className="header bg-gradient-info py-7 py-lg-8">
          <Container>
            <div className="header-body text-center mb-6">
              <Row className="justify-content-center">
                <Col lg="5" md="6">
                  <p><img
                    style={{ width: '100%' }}
                    alt="requestrr logo"
                    src={requestrrLogo}
                  />
                  </p>
                  <p style={{ color: 'white' }} className="mt-4">
                    Your favorite chatbot service for all your media needs
                  </p>
                </Col>
              </Row>
            </div>
          </Container>
          <div className="separator separator-bottom separator-skew zindex-100">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              preserveAspectRatio="none"
              version="1.1"
              viewBox="0 0 2560 100"
              x="0"
              y="0"
            >
              <polygon
                className="fill-default"
                points="2560 0 2560 100 0 100"
              />
            </svg>
          </div>
        </div>
        {/* Page content */}
        <Container className="mt--8 pb-5">
          <Row className="justify-content-center">
            {
              isLoading
                ? (<Col className="text-center" lg="5" md="7">
                  <Oval
                    type="Triangle"
                    color="#11cdef"
                    height={300}
                    width={300}
                    wrapperClass="svg-centre"
                  />
                </Col>)
                : <Switch>
                  {
                    !isLoading
                      ? props.isLoggedIn
                        ? null
                        : props.hasRegistered
                          ? getRoutes(routes, "/login")
                          : getRoutes(routes, "/register")
                      : null
                  }
                  {
                    !isLoading ?
                      props.isLoggedIn ?
                        <Route path="*" render={() => <Redirect to="/admin/" />} />
                        : props.hasRegistered ?
                          <Route path="*" render={() => <Redirect to="/auth/login" />} />
                          : <Route path="*" render={() => <Redirect to="/auth/register" />} />
                      : null
                  }
                </Switch>
            }
          </Row>
        </Container>
      </div>
      <AuthFooter />
    </>
  );
}

const mapPropsToState = state => {
  return {
    isLoggedIn: state.user.isLoggedIn,
    hasRegistered: state.user.hasRegistered,
  }
};

const mapPropsToAction = {
  validateLogin: validateLogin,
  validateRegistration: hasRegistered
};

export default connect(mapPropsToState, mapPropsToAction)(Auth);
