import {Component, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, FormBuilder, FormGroup, Validators} from "@angular/forms";
import {invalidCharactersValidator} from "../../../common/validators/invalid-characters.validator";
import {Location} from "@angular/common";
import {DeviceService} from "../../../core/device/device.service";
import {UserService} from "../../../core/user/user.service";
import {User, UserUpdate} from "../../../core/interfaces";
import {LanguageOptions} from "../../../home/register/language-options";
import {LangUtils} from "../../../core/lang/lang.utils";
import {ELanguage} from "../../../core/lang/language";
import {TranslocoService} from "@ngneat/transloco";
import {Observable, Subscription} from "rxjs";
import {ThemeService} from "../../../core/theme/theme.service";
import {ETheme} from "../../../core/theme/theme";

@Component({
  selector: 'sw-profile-page',
  templateUrl: './profile-page.component.html',
  styleUrls: ['./profile-page.component.css']
})
export class ProfilePageComponent implements OnInit, OnDestroy {

  updateForm: FormGroup;
  user: User;
  initialUser: User;
  inputFormName: AbstractControl | null;
  inputFormEmail: AbstractControl | null;
  languageOptions = LanguageOptions;
  darkThemeEnabled: boolean;
  mobileScreen$: Observable<boolean>;
  dataChanged = false;
  darkThemeEnabledInitial: boolean;
  private userSub: Subscription | undefined;
  private langSub: Subscription | undefined;
  private themeSub: Subscription | undefined;
  private formSub: Subscription | undefined;


  constructor(private readonly location: Location,
              private readonly deviceService: DeviceService,
              private readonly formBuilder: FormBuilder,
              private readonly userService: UserService,
              private readonly translocoService: TranslocoService,
              private readonly themeService: ThemeService) {
    this.userSub = this.userService.getUser().subscribe(u => this.initialUser = this.user = u as User);
    this.mobileScreen$ = this.deviceService.isMobileScreen();
  }

  ngOnInit(): void {
    this.darkThemeEnabledInitial = this.darkThemeEnabled = this.user.theme != ETheme.light;
    this.updateForm = this.formBuilder.group(
      {
        name: [this.user.name, [Validators.required, Validators.minLength(3), invalidCharactersValidator]],
        email: [this.user.email, [Validators.required, Validators.email]],
        language: [this.user.language, [Validators.required]],
        theme: [this.darkThemeEnabled]
      }
    )

    this.inputFormName = this.updateForm.get('name');
    this.inputFormEmail = this.updateForm.get('email');

    this.translocoService.setActiveLang(LangUtils.getLangFileName(this.user.language));
    this.langSub = this.updateForm.get('language')?.valueChanges.subscribe(
      (lang: ELanguage) => this.translocoService.setActiveLang(LangUtils.getLangFileName(lang))
    );

    this.themeSub = this.updateForm.get('theme')?.valueChanges.subscribe(
      () => {
        this.themeService.toggleTheme()
        this.darkThemeEnabled = !this.darkThemeEnabled;
      }
    );

    this.formSub = this.updateForm.valueChanges.subscribe(() => this.checkIfDataChanged());
  }

  ngOnDestroy(): void {
    this.userSub?.unsubscribe();
    this.langSub?.unsubscribe();
    this.themeSub?.unsubscribe();
    this.formSub?.unsubscribe();
  }

  checkIfDataChanged(): void {
    this.dataChanged = this.initialUser.name != this.inputFormName?.value.trim() ||
      this.initialUser.email != this.inputFormEmail?.value.trim() ||
      this.initialUser.language != this.updateForm.get('language')?.value ||
      this.darkThemeEnabledInitial != this.darkThemeEnabled;
  }

  update(): void {
    const updateData = this.updateForm.getRawValue() as UserUpdate;
    updateData.name = updateData.name.trim();
    updateData.email = updateData.email.trim();
    updateData.theme = this.darkThemeEnabled ? ETheme.dark : ETheme.light;
    console.log(updateData);

    // Todo toast dizendo para confirmar email
    // todo: ao registrar colocar mesmo toast
    // Todo: ao logar colocar toast dizendo que email não confirmado e redirecionar para tela de confirmar email ao clicar
  }

}
