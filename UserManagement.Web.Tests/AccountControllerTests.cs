using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using UserManagement.Controllers;
using UserManagement.Models.Account;
using System.Linq;

namespace UserManagement.Data.Tests;

public class AccountControllerTests
{
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
        _signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

        _controller = new AccountController(_signInManagerMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public void Login_ReturnsViewWithModel()
    {
        var result = _controller.Login();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<LoginViewModel>().Subject;

        model.Email.Should().BeEmpty();
        model.Password.Should().BeEmpty();
    }

    [Fact]
    public async Task Login_ValidCredentials_RedirectsToHome()
    {
        var model = new LoginViewModel { Email = "test@example.com", Password = "Password_123" };
        var user = new User { UserName = "testuser", Email = model.Email };

        _userManagerMock.Setup(userManager => userManager.FindByEmailAsync(model.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(signInManager => signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var result = await _controller.Login(model);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsViewWithError()
    {
        var model = new LoginViewModel { Email = "test@example.com", Password = "WrongPassword" };
        var user = new User { UserName = "testuser", Email = model.Email };
        _userManagerMock.Setup(userManager => userManager.FindByEmailAsync(model.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(signInManager => signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await _controller.Login(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeEquivalentTo(model);
        _controller.ModelState.IsValid.Should().BeFalse();
        var errorMessage = _controller.ModelState[string.Empty]?.Errors.FirstOrDefault()?.ErrorMessage;
        errorMessage.Should().Be("Invalid login attempt");
    }

    [Fact]
    public async Task Logout_RedirectsToLogin()
    {
        var result = await _controller.Logout();

        _signInManagerMock.Verify(sm => sm.SignOutAsync(), Times.Once);
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Login");
        redirectResult.ControllerName.Should().Be("Account");
    }
}
