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


function Account() {
  const [isSaving, setIsSaving] = useState(false);
  const [saveAttempted, setSaveAttempted] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [saveError, setSaveError] = useState("");
  const [password, setPassword] = useState("");
  const [hasPasswordChanged, setHasPasswordChanged] = useState(false);
  const [isPasswordInvalid, setIsPasswordInvalid] = useState(false);
  const [newPassword, setNewPassword] = useState("");
  const [hasNewPasswordChanged, setHasNewPasswordChanged] = useState(false);
  const [isNewPasswordInvalid, setIsNewPasswordInvalid] = useState(false);
  const [newPasswordConfirmation, setNewPasswordConfirmation] = useState("");
  const [hasNewPasswordConfirmationChanged, setHasNewPasswordConfirmationChanged] = useState(false);
  const [isNewPasswordConfirmationInvalid, setIsNewPasswordConfirmationInvalid] = useState(false);
  const [passwordsDoNotMatch, setPasswordsDoNotMatch] = useState(false);

  const dispatch = useDispatch();

  useEffect(() => {
    if (hasPasswordChanged) {
      triggerPasswordValidation();
    }
  }, [password]);


  useEffect(() => {
    if (hasNewPasswordChanged) {
      triggerNewPasswordValidation();
      triggerPasswordMatchValidation();
    }
  }, [newPassword]);


  useEffect(() => {
    if (hasNewPasswordConfirmationChanged) {
      triggerNewPasswordConfirmationValidation();
      triggerPasswordMatchValidation();
    }
  }, [newPasswordConfirmation]);




  const validateNewPassword = () => {
    return /\S/.test(newPassword);
  };

  const validateNewPasswordConfirmation = () => {
    return /\S/.test(newPasswordConfirmation);
  };

  const validatePasswordMatch = () => {
    return (!/\S/.test(newPassword) || !/\S/.test(newPasswordConfirmation)) || (newPassword === newPasswordConfirmation);
  };

  const validatePassword = () => {
    return /\S/.test(password);
  };



  const onPasswordChange = (event) => {
    setPassword(event.target.value);
    setHasPasswordChanged(true);
  };

  const triggerPasswordValidation = () => {
    setIsPasswordInvalid(!validatePassword());
  };


  const onNewPasswordChange = (event) => {
    setNewPassword(event.target.value);
    setHasNewPasswordChanged(true);
  };

  const triggerNewPasswordValidation = () => {
    setIsNewPasswordInvalid(!validateNewPassword());
  };

  const onNewPasswordConfirmationChange = (event) => {
    setNewPasswordConfirmation(event.target.value);
    setHasNewPasswordConfirmationChanged(true);
  };

  const triggerNewPasswordConfirmationValidation = () => {
    setIsNewPasswordConfirmationInvalid(!validateNewPasswordConfirmation());
  };

  const triggerPasswordMatchValidation = () => {
    setPasswordsDoNotMatch(!validatePasswordMatch());
  };


  const onSaving = (e) => {
    e.preventDefault();

    triggerPasswordValidation();
    triggerNewPasswordValidation();
    triggerNewPasswordConfirmationValidation();
    triggerPasswordMatchValidation();

    if (!isSaving) {
      if (validatePassword()
        && validateNewPassword()
        && validateNewPasswordConfirmation()
        && validatePasswordMatch()) {
        setIsSaving(true);

        dispatch(changePassword({
          'password': password,
          'newPassword': newPassword,
          'newPasswordConfirmation': newPasswordConfirmation,
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
        setSaveAttempted(true);
        setSaveError("Some fields are invalid, please fix them before saving.");
        setSaveSuccess(false);
      }
    }
  };



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
                        <FormGroup className={isPasswordInvalid ? "has-danger" : hasPasswordChanged ? "has-success" : ""}>
                          <label
                            className="form-control-label"
                            htmlFor="input-existing-password"
                          >
                            Existing password
                          </label>
                          <Input
                            value={password} onChange={onPasswordChange}
                            className="form-control-alternative"
                            id="input-existing-password"
                            placeholder="Enter the existing password"
                            type="password"
                          />
                          {
                            isPasswordInvalid ? (
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
                        <FormGroup className={isNewPasswordInvalid ? "has-danger" : hasNewPasswordChanged ? "has-success" : ""}>
                          <label
                            className="form-control-label"
                            htmlFor="input-new-password"
                          >
                            New password
                          </label>
                          <Input
                            value={newPassword} onChange={onNewPasswordChange}
                            className="form-control-alternative"
                            id="input-new-password"
                            placeholder="Enter the new password"
                            type="password"
                          />
                          {
                            isNewPasswordInvalid ? (
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
                        <FormGroup className={isNewPasswordConfirmationInvalid || passwordsDoNotMatch ? "has-danger" : hasNewPasswordConfirmationChanged ? "has-success" : ""}>
                          <label
                            className="form-control-label"
                            htmlFor="input-new-password-confirmation"
                          >
                            New password confirmation
                          </label>
                          <Input
                            value={newPasswordConfirmation} onChange={onNewPasswordConfirmationChange}
                            className="form-control-alternative"
                            id="input-new-password-confirmation"
                            placeholder="Enter the new password confirmation"
                            type="password"
                          />
                          {
                            isNewPasswordConfirmationInvalid ? (
                              <Alert className="mt-3" color="warning">
                                <strong>New password confirmation is required.</strong>
                              </Alert>)
                              : null
                          }
                          {
                            passwordsDoNotMatch ? (
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

export default Account;