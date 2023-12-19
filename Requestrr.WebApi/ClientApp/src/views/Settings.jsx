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
import { useDispatch } from 'react-redux';
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


function Settings() {

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [saveAttempted, setSaveAttempted] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [saveError, setSaveError] = useState("");
  const [port, setPort] = useState("");
  const [isPortValid, setIsPortValid] = useState(false);
  const [baseUrl, setBaseUrl] = useState("");
  const [isBaseUrlValid, setIsBaseUrlValid] = useState(false);
  const [disableAuthentication, setDisableAuthentication] = useState(false);

  const dispatch = useDispatch();


  useEffect(() => {
    dispatch(getSettings())
      .then(data => {

        setIsLoading(false);
        setPort(data.payload.port);
        setBaseUrl(data.payload.baseUrl);
        setDisableAuthentication(data.payload.disableAuthentication);
      });
  }, []);


  useEffect(() => {
    if (!isSaving)
      return;

    if (isPortValid && isBaseUrlValid) {
      dispatch(saveSettings({
        'port': port,
        'baseUrl': baseUrl,
        'disableAuthentication': disableAuthentication
      }))
        .then(data => {
          setIsSaving(false);

          if (data.ok) {
            setSaveAttempted(true);
            setSaveError("");
            setSaveSuccess(true);
          } else {
            let error = "An unknown error occurred while saving.";

            if (typeof (data.error) === "string")
              error = data.error;

            setSaveAttempted(true);
            setSaveError(error);
            setSaveSuccess(false);
          }
        });
    } else {
      setIsSaving(false);
      setSaveAttempted(true);
      setSaveError("Some fields are invalid, please fix them before saving.");
      setSaveSuccess(false);
    }
  }, [isSaving]);




  const validatePort = value => {
    return /^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$/.test(value);
  };

  const validatedBaseUrl = value => {
    return (!value || /^\s*$/.test(value)) || /^\/[/a-z0-9]+$/.test(value);
  };

  const onSaving = e => {
    e.preventDefault();

    if (!isSaving) {
      setIsPortValid(validatePort(port));
      setIsBaseUrlValid(validatedBaseUrl(baseUrl));
      setIsSaving(true);
    }
  };




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
              <CardBody className={isLoading ? "fade" : "fade show"}>
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
                          value={port}
                          validation={validatePort}
                          onChange={newPort => setPort(newPort)}
                          onValidate={isValid => setIsPortValid(isValid)} />
                      </Col>
                      <Col lg="6">
                        <ValidatedTextbox
                          name="Base Url"
                          placeholder="Enter base url"
                          alertClassName="mt-3 mb-0"
                          errorMessage="Base urls must start with /"
                          value={baseUrl}
                          validation={validatedBaseUrl}
                          onChange={newBaseUrl => setBaseUrl(newBaseUrl)}
                          onValidate={isValid => setIsBaseUrlValid(isValid)} />
                      </Col>
                    </Row>
                    <Row>
                      <Col md="12">
                        <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                          <Input
                            className="custom-control-input"
                            id="disableAuthentication"
                            type="checkbox"
                            onChange={e => { setDisableAuthentication(!disableAuthentication); }}
                            checked={disableAuthentication}
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
                            saveAttempted && !isSaving ?
                              !saveSuccess ? (
                                <Alert className="text-center" color="danger">
                                  <strong>{saveError}</strong>
                                </Alert>)
                                : <Alert className="text-center" color="success">
                                  <strong>Settings updated successfully.</strong>
                                </Alert>
                              : null
                          }
                        </FormGroup>
                        <FormGroup className="text-right">
                          <button className="btn btn-icon btn-3 btn-primary" onClick={onSaving} disabled={isSaving} type="button">
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

export default Settings;