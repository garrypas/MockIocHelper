# MockIocHelper 

IoC containers are great for reducing grunt work. However this generates some maintenance problems with testing. Imagine a CheckStatusService class with the following constructor:

    public CheckStatusService(IGenericModifierLogged<Check, CheckDal> repository, IOrderReader orderReader, ICheckHistoryService checkHistoryService, IOrderStatusService orderStatusService, ICheckEMails checkEMails)

Unfortunately when you write unit tests using a framework like MOQ to test this class you can often end up with code like this:

    _repoMock = new Mock<IGenericModifierLogged<Check,CheckDal>>();
	_orderReaderMock = new Mock<IOrderReader>();
	_checkHistoryServiceMock = new Mock<ICheckHistoryService>();
	_orderStatusServiceMock = new Mock<IOrderStatusService>();
	_checkEMailsMock = new Mock<ICheckEMails>();
	
    _service = new CheckStatusService(
         _repoMock,
         _orderReaderMock.Object,
         _checkHistoryServiceMock.Object,
         _orderStatusServiceMock.Object,
         _checkEMailsMock.Object);
		 
If you work in a team that re-factors code as they go you will know that a lot of time is wasted moving arguments around, and stitching code back together once it is broken up. In many teams the code above would be far worse.

This utility aims remove a lot of this, but mocking out the dependencies into a container under the covers, that can be accessed by type. The code above becomes the following:

    _mockHelper = new MockHelper();
	_service = _mockHelper.Create<CheckStatusService>();
	
Mocks are automatically created for each dependency. If you re-arrange arguments or break up your classes your tests still work fine. You can still easily get hold of mocks using the following syntax:

    _mockHelper.GetMock<IOrderReader>();
	
...or just get the object:

    _mockHelper.Object<IOrderReader>();
	
Occassionally you may need to override the automated mocking behaviour. Simply include these objects as arguments like so:

    var realOrderReader = new OrderReader();
	_service = _mockHelper.Create<CheckStatusService>(realOrderReader);
	
In the example above the IOrderReader argument will be substituted with your object rather than a mock.