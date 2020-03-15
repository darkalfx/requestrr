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
import { getSettings } from "../store/actions/MovieClientsActions"
import { saveDisabledClient } from "../store/actions/MovieClientsActions"
import { saveRadarrClient } from "../store/actions/MovieClientsActions"
import { saveOmbiClient } from "../store/actions/MovieClientsActions"
import ValidatedTextbox from "../components/Inputs/ValidatedTextbox"
import Dropdown from "../components/Inputs/Dropdown"
import Radarr from "../components/DownloadClients/Radarr"
import Ombi from "../components/DownloadClients/Ombi"

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

class Movies extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isSubmitted: false,
      isLoading: true,
      isSaving: false,
      saveAttempted: false,
      saveSuccess: false,
      saveError: "",
      client: "",
      command: "",
      radarr: {},
      isRadarrValid: false,
      ombi: {},
      isOmbiValid: false,
      isCommandValid: false,
    };

    this.onSaving = this.onSaving.bind(this);
    this.onClientChange = this.onClientChange.bind(this);
    this.validateNonEmptyString = this.validateNonEmptyString.bind(this);
  }

  componentDidMount() {
    this.props.getSettings()
      .then(data => {
        this.setState({
          isLoading: false,
          client: this.props.settings.client,
          radarr: this.props.settings.radarr,
          ombi: this.props.settings.ombi,
          command: this.props.settings.command,
        });
      });
  }

  validateNonEmptyString = value => {
    return /\S/.test(value);
  }

  onClientChange() {
    this.setState({
      radarr: this.props.settings.radarr,
      ombi: this.props.settings.ombi,
      savingAttempted: false,
      isSubmitted: false,
    });
  }

  onSaving = e => {
    e.preventDefault();
    this.setState({ isSubmitted: true });

    if (!this.state.isSaving
      && (this.state.client === "Disabled"
        || (this.state.client === "Radarr"
          && this.state.isRadarrValid
          && this.state.isCommandValid)
        || (this.state.client === "Ombi"
          && this.state.isOmbiValid
          && this.state.isCommandValid)
      )) {
      this.setState({ isSaving: true });

      let saveAction = null;

      if (this.state.client === "Disabled") {
        saveAction = this.props.saveDisabledClient();
      }
      else if (this.state.client === "Ombi") {
        saveAction = this.props.saveOmbiClient({
          ombi: this.state.ombi,
          command: this.state.command,
        });
      }
      else if (this.state.client === "Radarr") {
        saveAction = this.props.saveRadarrClient({
          radarr: this.state.radarr,
          command: this.state.command,
        });
      }

      saveAction.then(data => {
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
  }

  render() {
    return (
      <>
        <UserHeader title="Movies" description="This page is for configuring the connection between your bot and your favorite movie download client" />
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
                      General Settings
                    </h6>
                    <div className="pl-lg-4">
                      <Row>
                        <Col lg="6">
                          <Dropdown
                            name="Download Client"
                            value={this.state.client}
                            items={[{ name: "Disabled", value: "Disabled" }, { name: "Radarr", value: "Radarr" }, { name: "Ombi", value: "Ombi" }]}
                            onChange={newClient => { this.setState({ client: newClient }, this.onClientChange) }} />
                        </Col>
                      </Row>
                    </div>
                    {
                      this.state.client !== "Disabled"
                        ? <>
                          <hr className="my-4" />
                          {
                            this.state.client === "Ombi"
                              ? <>
                                <Ombi settings={this.state.ombi} onChange={newOmbi => this.setState({ ombi: newOmbi })} onValidate={isOmbiValid => this.setState({ isOmbiValid: isOmbiValid })} isSubmitted={this.state.isSubmitted} />
                              </>
                              : null
                          }
                          {
                            this.state.client === "Radarr"
                              ? <>
                                <Radarr settings={this.state.radarr} onChange={newRadarr => this.setState({ radarr: newRadarr })} onValidate={isRadarrValid => this.setState({ isRadarrValid: isRadarrValid })} isSubmitted={this.state.isSubmitted} />
                              </>
                              : null
                          }
                          <hr className="my-4" />
                          <h6 className="heading-small text-muted mb-4">
                            Chat Command Options
                          </h6>
                        </>
                        : null
                    }
                    <div className="pl-lg-4">
                      {
                        this.state.client !== "Disabled"
                          ? <>
                            <Row>
                              <Col lg="4">
                                <ValidatedTextbox
                                  name="Command"
                                  placeholder="Enter chat command"
                                  alertClassName="mt-3"
                                  errorMessage="A chat command is required."
                                  isSubmitted={this.state.isSubmitted}
                                  value={this.state.command}
                                  validation={this.validateNonEmptyString}
                                  onChange={newCommand => this.setState({ command: newCommand })}
                                  onValidate={isValid => this.setState({ isCommandValid: isValid })} />
                              </Col>
                              <Col lg="4">
                              </Col>
                            </Row>
                          </>
                          : null
                      }
                      <Row>
                        <Col>
                          <FormGroup className="mt-4">
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
    settings: state.movies
  }
};

const mapPropsToAction = {
  getSettings: getSettings,
  saveDisabledClient: saveDisabledClient,
  saveRadarrClient: saveRadarrClient,
  saveOmbiClient: saveOmbiClient,
};

export default connect(mapPropsToState, mapPropsToAction)(Movies);