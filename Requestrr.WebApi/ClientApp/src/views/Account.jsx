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
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { changePassword } from "../store/actions/UserActions"

// reactstrap components
import {
  Button,
  Card,
  CardHeader,
  CardBody,
  FormGroup,
  Form,
  Input,
  Container,
  Row,
  Col
} from "reactstrap";
// core components
import UserHeader from "../components/Headers/UserHeader.jsx";

class Account extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isSaving: false,
      saveAttempted: false,
      saveSuccess: false,
      saveError: "",
      password: "",
      hasPasswordChanged: false,
      isPasswordInvalid: false,
      newPassword: "",
      hasNewPasswordChanged: false,
      isNewPasswordInvalid: false,
      newPasswordConfirmation: "",
      hasNewPasswordConfirmationChanged: false,
      isNewPasswordConfirmationInvalid: false,
      passwordsDoNotMatch: false,
    };

    this.onSaving = this.onSaving.bind(this);
    this.onPasswordChange = this.onPasswordChange.bind(this);
    this.triggerPasswordValidation = this.triggerPasswordValidation.bind(this);
    this.validatePassword = this.validatePassword.bind(this);
    this.onNewPasswordChange = this.onNewPasswordChange.bind(this);
    this.triggerNewPasswordValidation = this.triggerNewPasswordValidation.bind(this);
    this.validateNewPassword = this.validateNewPassword.bind(this);
    this.onNewPasswordConfirmationChange = this.onNewPasswordConfirmationChange.bind(this);
    this.triggerNewPasswordConfirmationValidation = this.triggerNewPasswordConfirmationValidation.bind(this);
    this.validateNewPasswordConfirmation = this.validateNewPasswordConfirmation.bind(this);
    this.validatePasswordMatch = this.validatePasswordMatch.bind(this);
    this.triggerPasswordMatchValidation = this.triggerPasswordMatchValidation.bind(this);
  }

  onPasswordChange = event => {
    this.setState({
      password: event.target.value,
      hasPasswordChanged: true
    }, () => this.triggerPasswordValidation());
  }

  triggerPasswordValidation() {
    this.setState({
      isPasswordInvalid: !this.validatePassword()
    });
  }

  validatePassword() {
    return /\S/.test(this.state.password);
  }

  onNewPasswordChange = event => {
    this.setState({
      newPassword: event.target.value,
      hasNewPasswordChanged: true
    }, () => {
      this.triggerNewPasswordValidation();
      this.triggerPasswordMatchValidation();
    });
  }

  triggerNewPasswordValidation() {
    this.setState({
      isNewPasswordInvalid: !this.validateNewPassword()
    });
  }

  validateNewPassword() {
    return /\S/.test(this.state.newPassword);
  }

  onNewPasswordConfirmationChange = event => {
    this.setState({
      newPasswordConfirmation: event.target.value,
      hasNewPasswordConfirmationChanged: true
    }, () => {
      this.triggerNewPasswordConfirmationValidation();
      this.triggerPasswordMatchValidation();
    });
  }

  triggerNewPasswordConfirmationValidation() {
    this.setState({
      isNewPasswordConfirmationInvalid: !this.validateNewPasswordConfirmation()
    });
  }

  validateNewPasswordConfirmation() {
    return /\S/.test(this.state.newPasswordConfirmation);
  }

  triggerPasswordMatchValidation() {
    this.setState({
      passwordsDoNotMatch: !this.validatePasswordMatch()
    });
  }

  validatePasswordMatch() {
    return (!/\S/.test(this.state.newPassword) || !/\S/.test(this.state.newPasswordConfirmation)) || (this.state.newPassword === this.state.newPasswordConfirmation);
  }

  onSaving = e => {
    e.preventDefault();

    this.triggerPasswordValidation();
    this.triggerNewPasswordValidation();
    this.triggerNewPasswordConfirmationValidation();
    this.triggerPasswordMatchValidation();

    if (!this.state.isSaving) {
      if (this.validatePassword()
        && this.validateNewPassword()
        && this.validateNewPasswordConfirmation()
        && this.validatePasswordMatch()) {
        this.setState({ isSaving: true });

        this.props.changePassword({
          'password': this.state.password,
          'newPassword': this.state.newPassword,
          'newPasswordConfirmation': this.state.newPasswordConfirmation,
        })
          .then(data => {
            this.setState({ isSaving: false });

            if (data.ok) {
              this.setState({
                savingAttempted: true,
                savingError: "",
                savingSuccess: true
              });
            }
            else {
              var error = "An unknown error occurred while saving.";

              if (typeof (data.error) === "string")
                error = data.error;

              this.setState({
                savingAttempted: true,
                savingError: error,
                savingSuccess: false
              });
            }
          });
      }
      else {
        this.setState({
          savingAttempted: true,
          savingError: "Some fields are invalid, please fix them before saving.",
          savingSuccess: false
        });
      }
    }
  }

  render() {
    return (
      <>
        <UserHeader title="Account" description="This page is for configuring your admin account" />
        <Container className="mt--7" fluid>
          <Row>
            <Col className="order-xl-1" xl="12">
              <Card className="bg-secondary shadow">
                <CardHeader className="bg-white border-0">
                  <Row className="align-items-center">
                    <Col xs="8">
                      <h3 className="mb-0">Configuration</h3>
                    </Col>
                    <Col className="text-right" xs="4">
                      <Button
                        color="primary"
                        href="#pablo"
                        onClick={e => e.preventDefault()}
                        size="sm"
                      >
                        Settings
                      </Button>
                    </Col>
                  </Row>
                </CardHeader>
                <CardBody>
                  <Form className="complex">
                    <h6 className="heading-small text-muted mb-4">
                      Change Password
                    </h6>
                    <div className="pl-lg-4">
                      <Row>
                        <Col lg="12">
                          <FormGroup className={this.state.isPasswordInvalid ? "has-danger" : this.state.hasPasswordChanged ? "has-success" : ""}>
                            <label
                              className="form-control-label"
                              htmlFor="input-existing-password"
                            >
                              Existing password
                            </label>
                            <Input
                              value={this.state.password} onChange={this.onPasswordChange}
                              className="form-control-alternative"
                              id="input-existing-password"
                              placeholder="Enter the existing password"
                              type="password"
                            />
                            {
                              this.state.isPasswordInvalid ? (
                                <Alert className="mt-3" color="warning">
                                  <strong>Existing password is required.</strong>
                                </Alert>)
                                : null
                            }
                          </FormGroup>
                        </Col>
                      </Row>
                      <Row>
                        <Col lg="12">
                          <FormGroup className={this.state.isNewPasswordInvalid ? "has-danger" : this.state.hasNewPasswordChanged ? "has-success" : ""}>
                            <label
                              className="form-control-label"
                              htmlFor="input-new-password"
                            >
                              New password
                            </label>
                            <Input
                              value={this.state.newPassword} onChange={this.onNewPasswordChange}
                              className="form-control-alternative"
                              id="input-new-password"
                              placeholder="Enter the new password"
                              type="password"
                            />
                            {
                              this.state.isNewPasswordInvalid ? (
                                <Alert className="mt-3" color="warning">
                                  <strong>New password is required.</strong>
                                </Alert>)
                                : null
                            }
                          </FormGroup>
                        </Col>
                      </Row>
                      <Row>
                        <Col lg="12">
                          <FormGroup className={this.state.isNewPasswordConfirmationInvalid || this.state.passwordsDoNotMatch ? "has-danger" : this.state.hasNewPasswordConfirmationChanged ? "has-success" : ""}>
                            <label
                              className="form-control-label"
                              htmlFor="input-new-password-confirmation"
                            >
                              New password confirmation
                            </label>
                            <Input
                              value={this.state.newPasswordConfirmation} onChange={this.onNewPasswordConfirmationChange}
                              className="form-control-alternative"
                              id="input-new-password-confirmation"
                              placeholder="Enter the new password confirmation"
                              type="password"
                            />
                            {
                              this.state.isNewPasswordConfirmationInvalid ? (
                                <Alert className="mt-3" color="warning">
                                  <strong>New password confirmation is required.</strong>
                                </Alert>)
                                : null
                            }
                            {
                              this.state.passwordsDoNotMatch ? (
                                <Alert className="mt-3" color="warning">
                                  <strong>Password confirmation and password do not match.</strong>
                                </Alert>)
                                : null
                            }
                          </FormGroup>
                        </Col>
                      </Row>
                      <Row>
                        <Col>
                          <FormGroup>
                            {
                              this.state.savingAttempted && !this.state.isSaving ?
                                !this.state.savingSuccess ? (
                                  <Alert className="text-center" color="danger">
                                    <strong>{this.state.savingError}</strong>
                                  </Alert>)
                                  : <Alert className="text-center" color="success">
                                    <strong>Settings updated successfully.</strong>
                                  </Alert>
                                : null
                            }
                          </FormGroup>
                          <FormGroup className="text-right">
                            <button className="btn btn-icon btn-3 btn-primary" onClick={this.onSaving} disabled={this.state.isSaving} type="button">
                              <span className="btn-inner--icon"><i className="fas fa-save"></i></span>
                              <span className="btn-inner--text">Save Changes</span>
                            </button>
                          </FormGroup>
                        </Col>
                      </Row>
                    </div>
                  </Form>
                </CardBody>
              </Card>
            </Col>
          </Row>
        </Container>
      </>
    );
  }
}

const mapPropsToAction = {
  changePassword: changePassword
};

export default connect(null, mapPropsToAction)(Account);