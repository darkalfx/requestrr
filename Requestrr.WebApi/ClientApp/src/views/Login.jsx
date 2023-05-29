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
import React from "react";
import { Alert } from "reactstrap";
import { connect } from 'react-redux';
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

class Login extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isLoading: false,
      rememberMe: false,
      username: "",
      usernameChanged: false,
      usernameInvalid: false,
      password: "",
      passwordInvalid: false,
      passwordChanged: false,
      loginAttempted: false,
      loginSuccess: false,
      loginError: ""
    };

    this.onUsernameChange = this.onUsernameChange.bind(this);
    this.onPasswordChange = this.onPasswordChange.bind(this);
    this.onRememberMeChange = this.onRememberMeChange.bind(this);
    this.validateUsername = this.validateUsername.bind(this);
    this.validatePassword = this.validatePassword.bind(this);
    this.onLogin = this.onLogin.bind(this);
    this.triggerUsernameValidation = this.triggerUsernameValidation.bind(this);
    this.triggerPasswordValidation = this.triggerPasswordValidation.bind(this);
  }

  onUsernameChange = event => {
    this.setState({
      username: event.target.value,
      usernameChanged: true
    }, () => this.triggerUsernameValidation());
  }

  triggerUsernameValidation() {
    this.setState({
      usernameInvalid: !this.validateUsername()
    });
  }

  validateUsername() {
    return /\S/.test(this.state.username);
  }

  onPasswordChange = event => {
    this.setState({
      password: event.target.value,
      passwordChanged: true
    }, () => this.triggerPasswordValidation());
  }

  triggerPasswordValidation() {
    this.setState({
      passwordInvalid: !this.validatePassword()
    });
  }

  validatePassword() {
    return /\S/.test(this.state.password);
  }

  onRememberMeChange = event => {
    this.setState({
      rememberMe: !this.state.rememberMe
    });
  }

  onLogin = e => {
    e.preventDefault();

    this.triggerUsernameValidation();
    this.triggerPasswordValidation();

    if (this.validateUsername()
      && this.validatePassword()) {
      this.setState({ isLoading: true });

      this.props.login({
        username: this.state.username,
        password: this.state.password,
        rememberMe: this.state.rememberMe
      })
        .then(data => {
          if (data.ok) {
            this.props.history.push('/admin/');
          }
          else {
            this.setState({ isLoading: false });
            var error = "An unknown error occurred while logging in.";

            if (typeof (data.error) === "string")
              error = data.error;

            this.setState({
              loginAttempted: true,
              loginError: error,
              loginSuccess: false
            });
          }
        });
    }
  }

  render() {
    return (
      <>
        <Col lg="5" md="7">
          <Card className="bg-secondary shadow border-0">
            <CardBody className="px-lg-5 py-lg-5">
              <div className="text-center text-muted mb-4">
                <small>Use the form below to sign-in</small>
              </div>
              <Form onSubmit={this.onLogin} role="form">
                <FormGroup className={this.state.usernameInvalid ? "has-danger" : this.state.usernameChanged ? "has-success" : ""}>
                  <InputGroup className="input-group-alternative mb-3">
                    <InputGroupAddon addonType="prepend">
                      <InputGroupText>
                        <i className="ni ni-single-02" />
                      </InputGroupText>
                    </InputGroupAddon>
                    <Input placeholder="Username" value={this.state.username} onChange={this.onUsernameChange} type="text" />
                  </InputGroup>
                  {
                    this.state.usernameInvalid ? (
                      <Alert color="warning">
                        <strong>Username is required.</strong>
                      </Alert>)
                      : null
                  }
                </FormGroup>
                <FormGroup className={this.state.passwordInvalid ? "has-danger" : this.state.passwordChanged ? "has-success" : ""}>
                  <InputGroup className="input-group-alternative mb-3">
                    <InputGroupAddon addonType="prepend">
                      <InputGroupText>
                        <i className="ni ni-lock-circle-open" />
                      </InputGroupText>
                    </InputGroupAddon>
                    <Input placeholder="Password" onChange={this.onPasswordChange} type="password" />
                  </InputGroup>
                  {
                    this.state.passwordInvalid ? (
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
                    onChange={this.onRememberMeChange}
                    checked={this.state.rememberMe}
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
                    this.state.loginAttempted ?
                      !this.state.loginSuccess ? (
                        <Alert color="danger">
                          <strong>{this.state.loginError}</strong>
                        </Alert>)
                        : <Alert color="success">
                          <strong>Login successful.</strong>
                        </Alert>
                      : null
                  }
                </FormGroup>
                <div className="text-center">
                  <button type="submit" className="btn btn-icon btn-primary" disabled={this.state.isLoading}>
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
}

const mapPropsToAction = {
  login: login,
};

export default connect(null, mapPropsToAction)(Login);