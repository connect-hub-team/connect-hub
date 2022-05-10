export class User {
  constructor(
    public Id: string,
    public UserName: string,
    public IsMuted: boolean = false,
    public IsArchived: boolean = false,
  ) { }
}
