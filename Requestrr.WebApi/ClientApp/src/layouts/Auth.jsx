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
import { useDispatch, useSelector } from 'react-redux';
import { Route, Routes, Navigate } from "react-router-dom";
// reactstrap components
import { Container, Row, Col } from "reactstrap";
import { Oval } from 'react-loader-spinner'

// core components
import AuthNavbar from "../components/Navbars/AuthNavbar.jsx";
import AuthFooter from "../components/Footers/AuthFooter.jsx";
import { hasRegistered as validateRegistration } from "../store/actions/UserActions"
import { validateLogin } from "../store/actions/UserActions"

import routes from "../routes.js";
import requestrrLogo from "../assets/img/brand/requestrr.svg";

function Auth() {
  const [isLoading, setIsLoading] = useState(true);

  const reduxState = useSelector((state) => {
    return {
      isLoggedIn: state.user.isLoggedIn,
      hasRegistered: state.user.hasRegistered,
    }
  });
  const dispatch = useDispatch();


  useEffect(() => {
    document.body.classList.add("bg-default");

    dispatch(validateRegistration())
      .then(data => {
        dispatch(validateLogin())
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
            path={prop.path}
            element={prop.component}
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
                : <Routes>
                  {
                    !isLoading
                      ? reduxState.isLoggedIn
                        ? null
                        : reduxState.hasRegistered
                          ? getRoutes(routes, "/login")
                          : getRoutes(routes, "/register")
                      : null
                  }
                  {
                    !isLoading ?
                      reduxState.isLoggedIn ?
                        <Route path="*" element={<Navigate to="/admin" />} />
                        : reduxState.hasRegistered ?
                          <Route path="*" element={<Navigate to="/auth/login" />} />
                          : <Route path="*" element={<Navigate to="/auth/register" />} />
                      : null
                  }
                </Routes>
            }
          </Row>
        </Container>
      </div>
      <AuthFooter />
    </>
  );
}

export default Auth;
