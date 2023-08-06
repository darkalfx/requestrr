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
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Alert } from "reactstrap";
import { useDispatch } from 'react-redux';
import { login } from "../store/actions/UserActions"

// reactstrap components
import {
  Card,
  CardBody,
  FormGroup,
  Form,
  Input,
  InputGroupAddon,
  InputGroupText,
  InputGroup,
  Col
} from "reactstrap";


function Login() {
  const [isLoading, setIsLoading] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [username, setUsername] = useState("");
  const [usernameChanged, setUsernameChanged] = useState(false);
  const [usernameInvalid, setUsernameInvalid] = useState(false);
  const [password, setPassword] = useState("");
  const [passwordInvalid, setPasswordInvalid] = useState(false);
  const [passwordChanged, setPasswordChanged] = useState(false);
  const [loginAttempted, setLoginAttempted] = useState(false);
  const [loginSuccess, setLoginSuccess] = useState(false);
  const [loginError, setLoginError] = useState("");

  const history = useNavigate();
  const dispatch = useDispatch();


  useEffect(() => {
    if (usernameChanged)
      triggerUsernameValidation();
  }, [usernameChanged]);


  useEffect(() => {
    if (passwordChanged)
      triggerPasswordValidation();
  }, [passwordChanged]);


  const validateUsername = () => {
    return /\S/.test(username);
  };


  const validatePassword = () => {
    return /\S/.test(password);
  };



  const triggerUsernameValidation = () => {
    setUsernameInvalid(!validateUsername());
  };

  const onUsernameChange = event => {
    setUsername(event.target.value);
    setUsernameChanged(true);
  };


  const onPasswordChange = event => {
    setPassword(event.target.value);
    setPasswordChanged(true);
  };


  const triggerPasswordValidation = () => {
    setPasswordInvalid(!validatePassword());
  };


  const onRememberMeChange = event => {
    setRememberMe(!rememberMe)
  };


  const onLogin = (e) => {
    e.preventDefault();

    triggerUsernameValidation();
    triggerPasswordValidation();

    if (validateUsername()
      && validatePassword()) {
      setIsLoading(true);

      dispatch(
        login({
          username: username,
          password: password,
          rememberMe: rememberMe
        }))
        .then(data => {
          if (data.ok) {
            history('/admin/');
          } else {
            setIsLoading(false);
            let error = "An unknown error occurred while logging in.";

            if (typeof (data.error) === "string")
              error = data.error;

            setLoginAttempted(true);
            setLoginError(error);
            setLoginSuccess(false);
          }
        });
    }
  };





  return (
    <>
      <Col lg="5" md="7">
        <Card className="bg-secondary shadow border-0">
          <CardBody className="px-lg-5 py-lg-5">
            <div className="text-center text-muted mb-4">
              <small>Use the form below to sign-in</small>
            </div>
            <Form onSubmit={onLogin} role="form">
              <FormGroup className={usernameInvalid || (usernameChanged && username === "") ? "has-danger" : usernameChanged ? "has-success" : ""}>
                <InputGroup className="input-group-alternative mb-3">
                  <InputGroupAddon addonType="prepend">
                    <InputGroupText>
                      <i className="ni ni-single-02" />
                    </InputGroupText>
                  </InputGroupAddon>
                  <Input placeholder="Username" value={username} onChange={onUsernameChange} type="text" />
                </InputGroup>
                {
                  usernameInvalid ? (
                    <Alert color="warning">
                      <strong>Username is required.</strong>
                    </Alert>)
                    : null
                }
              </FormGroup>
              <FormGroup className={passwordInvalid || (passwordChanged && password === "") ? "has-danger" : passwordChanged ? "has-success" : ""}>
                <InputGroup className="input-group-alternative mb-3">
                  <InputGroupAddon addonType="prepend">
                    <InputGroupText>
                      <i className="ni ni-lock-circle-open" />
                    </InputGroupText>
                  </InputGroupAddon>
                  <Input placeholder="Password" onChange={onPasswordChange} type="password" />
                </InputGroup>
                {
                  passwordInvalid ? (
                    <Alert color="warning">
                      <strong>Password is required.</strong>
                    </Alert>)
                    : null
                }
              </FormGroup>
              <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                <Input
                  className="custom-control-input"
                  id="rememberMe"
                  type="checkbox"
                  onChange={onRememberMeChange}
                  checked={rememberMe}
                />
                <label
                  className="custom-control-label"
                  htmlFor="rememberMe"
                >
                  <span className="text-muted">Remember me</span>
                </label>
              </FormGroup>
              <FormGroup>
                {
                  loginAttempted ?
                    !loginSuccess ? (
                      <Alert color="danger">
                        <strong>{loginError}</strong>
                      </Alert>)
                      : <Alert color="success">
                        <strong>Login successful.</strong>
                      </Alert>
                    : null
                }
              </FormGroup>
              <div className="text-center">
                <button type="submit" className="btn btn-icon btn-primary" disabled={isLoading}>
                  <span className="btn-inner--icon"><i className="fas fa-sign-in-alt"></i></span>
                  <span className="btn-inner--text">Sign in</span>
                </button>
              </div>
            </Form>
          </CardBody>
        </Card>
      </Col>
    </>
  );
}

export default Login;