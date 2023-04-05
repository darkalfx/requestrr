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
import { useNavigate } from "react-router-dom";
import { useDispatch } from 'react-redux';
import { Alert } from "reactstrap";
import { register } from "../store/actions/UserActions"

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


function Register() {
  const [isLoading, setIsLoading] = useState(false);
  const [username, setUsername] = useState("");
  const [usernameChanged, setUsernameChanged] = useState(false);
  const [password, setPassword] = useState("");
  const [passwordChanged, setPasswordChanged] = useState(false);
  const [passwordConfirmation, setPasswordConfirmation] = useState("");
  const [passwordConfirmationChanged, setPasswordConfirmationChanged] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [usernameInvalid, setUsernameInvalid] = useState(false);
  const [passwordInvalid, setPasswordInvalid] = useState(false);
  const [passwordConfirmationInvalid, setPasswordConfirmationInvalid] = useState(false);
  const [passwordsDoNotMatch, setPasswordsDoNotMatch] = useState(false);
  const [registrationAttempted, setRegistrationAttempted] = useState(false);
  const [registrationSuccess, setRegistrationSuccess] = useState(false);
  const [registrationError, setRegistrationError] = useState("");

  const history = useNavigate();
  const dispatch = useDispatch();


  useEffect(() => {
    triggerUsernameValidation();
  }, [usernameChanged]);


  useEffect(() => {
    triggerPasswordValidation();
    triggerPasswordMatchValidation();
  }, [passwordChanged]);

  useEffect(() => {
    triggerPasswordConfirmationValidation();
    triggerPasswordMatchValidation();
  }, [passwordConfirmationChanged]);



  const validateUsername = () => {
    return /\S/.test(username);
  };

  const validatePassword = () => {
    return /\S/.test(password);
  };

  const validatePasswordConfirmation = () => {
    return /\S/.test(passwordConfirmation);
  };

  const validatePasswordMatch = () => {
    return (!/\S/.test(password) || !/\S/.test(passwordConfirmation)) || (password === passwordConfirmation);
  };



  const onUsernameChange = event => {
    setUsername(event.target.value);
    setUsernameChanged(true);
  };

  const triggerUsernameValidation = () => {
    setUsernameInvalid(!validateUsername());
  };

  const onPasswordChange = event => {
    setPassword(event.target.value);
    setPasswordChanged(true);
  };

  const triggerPasswordValidation = () => {
    setPasswordInvalid(!validatePassword());
  };

  const onPasswordConfirmationChange = event => {
    setPasswordConfirmation(event.target.value);
    setPasswordConfirmationChanged(true);
  };

  const triggerPasswordConfirmationValidation = () => {
    setPasswordConfirmationInvalid(!validatePasswordConfirmation());
  };

  const triggerPasswordMatchValidation = () => {
    setPasswordsDoNotMatch(!validatePasswordMatch());
  };

  const onRememberMeChange = event => {
    setRememberMe(!rememberMe);
  };

  const onRegister = e => {
    e.preventDefault();

    triggerUsernameValidation();
    triggerPasswordValidation();
    triggerPasswordConfirmationValidation();
    triggerPasswordMatchValidation();

    if (validateUsername()
      && validatePassword()
      && validatePasswordConfirmation()
      && validatePasswordMatch()) {

      setIsLoading(true);

      dispatch(register({
        username: username,
        password: password,
        passwordConfirmation: passwordConfirmation,
        rememberMe: rememberMe
      }))
        .then(data => {
          if (data.ok) {
            history('/admin/');
          }
          else {
            setIsLoading(false);
            let error = "An unknown error occurred while registrating.";

            if (typeof (data.error) === "string")
              error = data.error;

            setRegistrationAttempted(true);
            setRegistrationError(error);
            setRegistrationSuccess(false);
          }
        });
    }
  };



  return (
    <>
      <Col lg="6" md="8">
        <Card className="bg-secondary shadow border-0">
          <CardBody className="px-lg-5 py-lg-5">
            <div className="text-center text-muted mb-4">
              <small>Use the form below to create the admin account</small>
            </div>
            <Form role="form">
              <FormGroup className={usernameInvalid ? "has-danger" : usernameChanged ? "has-success" : ""}>
                <InputGroup className="input-group-alternative mb-3">
                  <InputGroupAddon addonType="prepend">
                    <InputGroupText>
                      <i className="ni ni-single-02" />
                    </InputGroupText>
                  </InputGroupAddon>
                  <Input name="username" value={username} onChange={onUsernameChange} placeholder="Username" type="text" />
                </InputGroup>
                {
                  usernameInvalid ? (
                    <Alert color="warning">
                      <strong>Username is required.</strong>
                    </Alert>)
                    : null
                }
              </FormGroup>
              <FormGroup className={passwordInvalid ? "has-danger" : passwordChanged ? "has-success" : ""}>
                <InputGroup className="input-group-alternative mb-3">
                  <InputGroupAddon addonType="prepend">
                    <InputGroupText>
                      <i className="ni ni-lock-circle-open" />
                    </InputGroupText>
                  </InputGroupAddon>
                  <Input value={password} onChange={onPasswordChange} placeholder="Password" type="password" />
                </InputGroup>
                {
                  passwordInvalid ? (
                    <Alert color="warning">
                      <strong>Password is required.</strong>
                    </Alert>)
                    : null
                }
              </FormGroup>
              <FormGroup className={passwordConfirmationInvalid || passwordsDoNotMatch ? "has-danger" : passwordConfirmationChanged ? "has-success" : ""}>
                <InputGroup className="input-group-alternative mb-3">
                  <InputGroupAddon addonType="prepend">
                    <InputGroupText>
                      <i className="ni ni-lock-circle-open" />
                    </InputGroupText>
                  </InputGroupAddon>
                  <Input value={passwordConfirmation} onChange={onPasswordConfirmationChange} placeholder="Confirm Password" type="password" />
                </InputGroup>
                {
                  passwordConfirmationInvalid ? (
                    <Alert color="warning">
                      <strong>Password confirmation is required.</strong>
                    </Alert>)
                    : null
                }
                {
                  passwordsDoNotMatch ? (
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
                  registrationAttempted ?
                    !registrationSuccess ? (
                      <Alert color="danger">
                        <strong>{registrationError}</strong>
                      </Alert>)
                      : <Alert color="success">
                        <strong>Registration successful.</strong>
                      </Alert>
                    : null
                }
              </FormGroup>
              <div className="text-center">
                <button type="button" class="btn btn-icon btn-primary" onClick={onRegister} disabled={isLoading}>
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

export default Register;