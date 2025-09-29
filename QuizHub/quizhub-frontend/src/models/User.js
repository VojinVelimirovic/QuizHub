export class LoginRequest {
  constructor(usernameOrEmail, password) {
    this.usernameOrEmail = usernameOrEmail;
    this.password = password;
  }
}