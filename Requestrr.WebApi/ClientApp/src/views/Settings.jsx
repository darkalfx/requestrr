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
import { getSettings } from "../store/actions/SettingsActions"
import { saveSettings } from "../store/actions/SettingsActions"
import ValidatedTextbox from "../components/Inputs/ValidatedTextbox"

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

class Settings extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isLoading: true,
      isSaving: false,
      isSubmitted: false,
      saveSuccess: false,
      saveError: "",
      port: "",
      isPortValid: false,
      baseUrl: "",
      isBaseUrlValid: false,
      disableAuthentication: false,
    };

    this.onSaving = this.onSaving.bind(this);
    this.validatePort = this.validatePort.bind(this);
    this.validatedBaseUrl = this.validatedBaseUrl.bind(this);
  }

  componentDidMount() {
    this.props.getSettings()
      .then(data => {
        this.setState({
          isLoading: false,
          port: this.props.settings.port,
          baseUrl: this.props.settings.baseUrl,
          disableAuthentication: this.props.settings.disableAuthentication,
        });
      });
  }

  validatePort = value => {
    return /^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$/.test(value);
  }

  validatedBaseUrl = value => {
    return (!value || /^\s*$/.test(value)) || /^\/[/a-z0-9]+$/.test(value);
  }

  onSaving = e => {
    e.preventDefault();

    if (!this.state.isSaving) {
      this.setState({
        isSaving: true,
        isPortValid: this.validatePort(this.state.port),
        isBaseUrlValid: this.validatedBaseUrl(this.state.baseUrl)
      }, () => {
        if (this.state.isPortValid && this.state.isBaseUrlValid) {
          this.props.saveSettings({
            'port': this.state.port,
            'baseUrl': this.state.baseUrl,
            'disableAuthentication': this.state.disableAuthentication
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
            isSaving: false,
            savingAttempted: true,
            savingError: "Some fields are invalid, please fix them before saving.",
            savingSuccess: false
          });
        }
      });
    }
  }

  render() {
    return (
      <>
        <UserHeader title="Settings" description="This page is for configuring general settings" />
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
                <CardBody className={this.state.isLoading ? "fade" : "fade show"}>
                  <Form className="complex">
                    <h6 className="heading-small text-muted mb-4">
                      Web portal
                    </h6>
                    <div className="pl-lg-4">
                      <Row>
                        <Col lg="6">
                          <ValidatedTextbox
                            name="Port"
                            placeholder="Enter port"
                            alertClassName="mt-3 mb-0"
                            errorMessage="Please enter a valid port."
                            value={this.state.port}
                            validation={this.validatePort}
                            onChange={newPort => this.setState({ port: newPort })}
                            onValidate={isValid => this.setState({ isPortValid: isValid })} />
                        </Col>
                        <Col lg="6">
                          <ValidatedTextbox
                            name="Base Url"
                            placeholder="Enter base url"
                            alertClassName="mt-3 mb-0"
                            errorMessage="Base urls must start with /"
                            value={this.state.baseUrl}
                            validation={this.validatedBaseUrl}
                            onChange={newBaseUrl => this.setState({ baseUrl: newBaseUrl })}
                            onValidate={isValid => this.setState({ isBaseUrlValid: isValid })} />
                        </Col>
                      </Row>
                      <Row>
                        <Col md="12">
                          <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                            <Input
                              className="custom-control-input"
                              id="disableAuthentication"
                              type="checkbox"
                              onChange={e => { this.setState({ disableAuthentication: !this.state.disableAuthentication }); }}
                              checked={this.state.disableAuthentication}
                            />
                            <label
                              className="custom-control-label"
                              htmlFor="disableAuthentication"
                            >
                              <span className="text-muted">Disable web portal authentication</span>
                            </label>
                          </FormGroup>
                        </Col>
                      </Row>
                      <Row>
                        <Col>
                          <FormGroup>
                            <Alert color="warning">
                              <strong>Warning:</strong> You must restart the bot for any changes made on this page to take effect.
                            </Alert>
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

const mapPropsToState = state => {
  return {
    settings: state.settings
  }
};

const mapPropsToAction = {
  getSettings: getSettings,
  saveSettings: saveSettings,
};

export default connect(mapPropsToState, mapPropsToAction)(Settings);