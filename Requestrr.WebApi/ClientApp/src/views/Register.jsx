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
import { Oval } from 'react-loader-spinner'
import { Redirect } from 'react-router-dom'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { register } from "../store/actions/UserActions"
import { hasRegistered } from "../store/actions/UserActions"

// reactstrap components
import {
  Button,
  Card,
  CardHeader,
  CardBody,
  FormGroup,
  Form,
  Input,
  InputGroupAddon,
  InputGroupText,
  InputGroup,
  Row,
  Col
} from "reactstrap";

class Register extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isLoading: false,
      username: "",
      usernameChanged: false,
      password: "",
      passwordChanged: false,
      passwordConfirmation: "",
      passwordConfirmationChanged: false,
      rememberMe: false,
      usernameInvalid: false,
      passwordInvalid: false,
      passwordConfirmationInvalid: false,
      passwordsDoNotMatch: false,
      registrationAttempted: false,
      registrationSuccess: false,
      registrationError: ""
    };

    this.onUsernameChange = this.onUsernameChange.bind(this);
    this.onPasswordChange = this.onPasswordChange.bind(this);
    this.onPasswordConfirmationChange = this.onPasswordConfirmationChange.bind(this);
    this.onRememberMeChange = this.onRememberMeChange.bind(this);
    this.validateUsername = this.validateUsername.bind(this);
    this.validatePassword = this.validatePassword.bind(this);
    this.validatePasswordConfirmation = this.validatePasswordConfirmation.bind(this);
    this.validatePasswordMatch = this.validatePasswordMatch.bind(this);
    this.onRegister = this.onRegister.bind(this);
    this.triggerUsernameValidation = this.triggerUsernameValidation.bind(this);
    this.triggerPasswordValidation = this.triggerPasswordValidation.bind(this);
    this.triggerPasswordConfirmationValidation = this.triggerPasswordConfirmationValidation.bind(this);
    this.triggerPasswordMatchValidation = this.triggerPasswordMatchValidation.bind(this);
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
    }, () => {
      this.triggerPasswordValidation();
      this.triggerPasswordMatchValidation();
    });
  }

  triggerPasswordValidation() {
    this.setState({
      passwordInvalid: !this.validatePassword()
    });
  }

  validatePassword() {
    return /\S/.test(this.state.password);
  }

  onPasswordConfirmationChange = event => {
    this.setState({
      passwordConfirmation: event.target.value,
      passwordConfirmationChanged: true
    }, () => {
      this.triggerPasswordConfirmationValidation();
      this.triggerPasswordMatchValidation();
    });
  }

  triggerPasswordConfirmationValidation() {
    this.setState({
      passwordConfirmationInvalid: !this.validatePasswordConfirmation()
    });
  }

  validatePasswordConfirmation() {
    return /\S/.test(this.state.passwordConfirmation);
  }

  triggerPasswordMatchValidation() {
    this.setState({
      passwordsDoNotMatch: !this.validatePasswordMatch()
    });
  }

  validatePasswordMatch() {
    return (!/\S/.test(this.state.password) || !/\S/.test(this.state.passwordConfirmation)) || (this.state.password === this.state.passwordConfirmation);
  }

  onRememberMeChange = event => {
    this.setState({
      rememberMe: !this.state.rememberMe
    });
  }

  onRegister = e => {
    e.preventDefault();

    this.triggerUsernameValidation();
    this.triggerPasswordValidation();
    this.triggerPasswordConfirmationValidation();
    this.triggerPasswordMatchValidation();

    if (this.validateUsername()
      && this.validatePassword()
      && this.validatePasswordConfirmation()
      && this.validatePasswordMatch()) {
      this.setState({ isLoading: true });

      this.props.register({
        username: this.state.username,
        password: this.state.password,
        passwordConfirmation: this.state.passwordConfirmation,
        rememberMe: this.state.rememberMe
      })
        .then(data => {
          if (data.ok) {
            this.props.history.push('/admin/');
          }
          else {
            this.setState({ isLoading: false });
            var error = "An unknown error occurred while registrating.";

            if (typeof (data.error) === "string")
              error = data.error;

            this.setState({
              registrationAttempted: true,
              registrationError: error,
              registrationSuccess: false
            });
          }
        });
    }
  }

  render() {
    return (
      <>
        <Col lg="6" md="8">
          <Card className="bg-secondary shadow border-0">
            <CardBody className="px-lg-5 py-lg-5">
              <div className="text-center text-muted mb-4">
                <small>Use the form below to create the admin account</small>
              </div>
              <Form role="form">
                <FormGroup className={this.state.usernameInvalid ? "has-danger" : this.state.usernameChanged ? "has-success" : ""}>
                  <InputGroup className="input-group-alternative mb-3">
                    <InputGroupAddon addonType="prepend">
                      <InputGroupText>
                        <i className="ni ni-single-02" />
                      </InputGroupText>
                    </InputGroupAddon>
                    <Input name="username" value={this.state.username} onChange={this.onUsernameChange} placeholder="Username" type="text" />
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
                    <Input value={this.state.password} onChange={this.onPasswordChange} placeholder="Password" type="password" />
                  </InputGroup>
                  {
                    this.state.passwordInvalid ? (
                      <Alert color="warning">
                        <strong>Password is required.</strong>
                      </Alert>)
                      : null
                  }
                </FormGroup>
                <FormGroup className={this.state.passwordConfirmationInvalid || this.state.passwordsDoNotMatch ? "has-danger" : this.state.passwordConfirmationChanged ? "has-success" : ""}>
                  <InputGroup className="input-group-alternative mb-3">
                    <InputGroupAddon addonType="prepend">
                      <InputGroupText>
                        <i className="ni ni-lock-circle-open" />
                      </InputGroupText>
                    </InputGroupAddon>
                    <Input value={this.state.passwordConfirmation} onChange={this.onPasswordConfirmationChange} placeholder="Confirm Password" type="password" />
                  </InputGroup>
                  {
                    this.state.passwordConfirmationInvalid ? (
                      <Alert color="warning">
                        <strong>Password confirmation is required.</strong>
                      </Alert>)
                      : null
                  }
                  {
                    this.state.passwordsDoNotMatch ? (
                      <Alert color="warning">
                        <strong>Password confirmation and password do not match.</strong>
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
                    this.state.registrationAttempted ?
                      !this.state.registrationSuccess ? (
                        <Alert color="danger">
                          <strong>{this.state.registrationError}</strong>
                        </Alert>)
                        : <Alert color="success">
                          <strong>Registration successful.</strong>
                        </Alert>
                      : null
                  }
                </FormGroup>
                <div className="text-center">
                  <button type="button" class="btn btn-icon btn-primary" onClick={this.onRegister} disabled={this.state.isLoading}>
                    <span className="btn-inner--icon"><i className="fas fa-file-signature"></i></span>
                    <span className="btn-inner--text">Create account</span>
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
  register: register,
};

export default connect(null, mapPropsToAction)(Register);