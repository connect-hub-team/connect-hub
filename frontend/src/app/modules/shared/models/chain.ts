import { ChainType } from "./chain-type.enum";
import { User } from "./user";

// dialog model
export class ChainDto {
  constructor(
    public Title: string,
    public Avatar: string,
    public Type: ChainType,
    public Users: User[],
  ) { }
}
